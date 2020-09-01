using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    /// <summary>
    /// Converts processes to ruby blocks
    /// </summary>
    public static class RubyBlockConversion
    {
        /// <summary>
        /// Tries to convert a process into a collection of ruby blocks
        /// </summary>
        public static Result<IRubyBlock> TryConvert(IRunnableProcess process)
        {
            if (process is IConstantRunnableProcess)
            {
                var dynamicConstant = ConvertConstant(process as dynamic);
                var constantRubyBlock = (IRubyBlock)dynamicConstant;

                return Result.Success(constantRubyBlock);
            }
            if (process is BlockProcess blockProcess)
                return Result.Success<IRubyBlock>(blockProcess.Block);
            if (process is IRubyScriptProcess rubyScriptProcess)
                return rubyScriptProcess.TryConvert();

            foreach (var coreMethodConverter in CoreMethodConverters.Value)
            {
                var r = coreMethodConverter.Convert(process);
                if (r.IsSuccess)
                    return r;
            }


            return Result.Failure<IRubyBlock>($"Could not convert '{process.Name}' to ruby block.");
        }

        //TODO many special cases

        private static readonly Lazy<IReadOnlyCollection<ICoreMethodConverter>> CoreMethodConverters =
            new Lazy<IReadOnlyCollection<ICoreMethodConverter>>(()=>
                Assembly.GetAssembly(typeof(ICoreMethodConverter))!
                    .GetTypes()
                    .Where(x=>x.IsClass && !x.IsAbstract)
                    .Where(x=>typeof(ICoreMethodConverter).IsAssignableFrom(x))
                    .Select(Activator.CreateInstance)
                    .Cast<ICoreMethodConverter>().ToList());

        private static ConstantRubyBlock<T> ConvertConstant<T>(Constant<T> constant) => new ConstantRubyBlock<T>(constant.Value);
    }


    /// <summary>
    /// Converts a core method to a ruby block
    /// </summary>
    internal interface ICoreMethodConverter
    {
        Result<IRubyBlock> Convert(IRunnableProcess process);
    }


    internal abstract class CoreMethodConverter<TProcess> : ICoreMethodConverter, IRubyFunction
    {
        /// <inheritdoc />
        public Result<IRubyBlock> Convert(IRunnableProcess process)
        {
            if (process is TProcess tProcess)
            {
                var argumentValuesResult =
                    GetArgumentBlocks(tProcess).Select(x =>
                            RubyBlockConversion.TryConvert(x.argumentProcess)
                                .BindCast<IRubyBlock, ITypedRubyBlock>()
                                .Map(rubyBlock => (x.parameter, rubyBlock))

                        ).Combine()
                        .Map(enumerable =>
                            enumerable.ToDictionary(
                                x => x.parameter,
                                x => x.rubyBlock));

                if (argumentValuesResult.IsFailure)
                    return argumentValuesResult.ConvertFailure<IRubyBlock>();

                var block = Create(argumentValuesResult.Value);

                return Result.Success(block);
            }
            return Result.Failure<IRubyBlock>("Could not convert.");
        }

        protected abstract IRubyBlock Create(IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> dictionary);

        protected abstract IEnumerable<(RubyFunctionParameter parameter, IRunnableProcess argumentProcess)> GetArgumentBlocks(TProcess process);


        /// <inheritdoc />
        public abstract string FunctionName { get; }

        /// <inheritdoc />
        public abstract string FunctionText { get; }

        /// <inheritdoc />
        public bool RequireUtilities => false;

        /// <inheritdoc />
        public abstract IReadOnlyCollection<RubyFunctionParameter> Arguments { get; }

        /// <inheritdoc />
        public Version RequiredNuixVersion => NuixVersionHelper.DefaultRequiredVersion;

        /// <inheritdoc />
        public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures => ImmutableList<NuixFeature>.Empty;
    }
    internal abstract class CoreUnitMethodConverter<TProcess> : CoreMethodConverter<TProcess>, IRubyFunction<Unit>
        where TProcess : ICompoundRunnableProcess<Unit>
    {
        /// <inheritdoc />
        protected override IRubyBlock Create(IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> dictionary) => new UnitCompoundRubyBlock(this, dictionary);
    }

    internal abstract class CoreTypedMethodConverter<TProcess, TOutput> : CoreMethodConverter<TProcess>, IRubyFunction<TOutput>
        where TProcess : ICompoundRunnableProcess<TOutput>
    {
        /// <inheritdoc />
        protected override IRubyBlock Create(IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> dictionary) => new TypedCompoundRubyBlock<TOutput>(this, dictionary);
    }


    internal class AssertTrueConverter : CoreUnitMethodConverter<AssertTrue>
    {
        /// <inheritdoc />
        protected override IEnumerable<(RubyFunctionParameter parameter, IRunnableProcess argumentProcess)> GetArgumentBlocks(AssertTrue process)
        {
            yield return (TestParameter, process.Test);
        }

        /// <inheritdoc />
        public override string FunctionName => "assertTrue";

        /// <inheritdoc />
        public override string FunctionText { get; } = $@"if !{TestParameter.ParameterName}
    raise 'Assertion failed'";

        private static readonly RubyFunctionParameter TestParameter
            = new RubyFunctionParameter("testArg", false);

        /// <inheritdoc />
        public override IReadOnlyCollection<RubyFunctionParameter> Arguments { get; } = new[] {TestParameter};
    }

    internal class NotConverter : CoreTypedMethodConverter<Not, bool>
    {
        /// <inheritdoc />
        protected override IEnumerable<(RubyFunctionParameter parameter, IRunnableProcess argumentProcess)> GetArgumentBlocks(Not process)
        {
            yield return (BooleanParameter, process.Boolean);
        }

        /// <inheritdoc />
        public override string FunctionName => "negate";

        private static readonly RubyFunctionParameter BooleanParameter
            = new RubyFunctionParameter("booleanArg", false);

        /// <inheritdoc />
        public override string FunctionText { get; } = $"not {BooleanParameter.ParameterName}";

        /// <inheritdoc />
        public override IReadOnlyCollection<RubyFunctionParameter> Arguments { get; } = new[] {BooleanParameter};
    }

}