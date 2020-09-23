using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal abstract class CoreMethodConverter<TProcess> : ICoreMethodConverter, IRubyFunction
    {
        /// <inheritdoc />
        public Result<IRubyBlock> Convert(IRunnableProcess process)
        {
            if (process is TProcess tProcess)
            {
                var argumentValuesResult =
                    GetArgumentBlocks(tProcess).Select(x =>
                            RubyBlockConversion.TryConvert(x.argumentProcess, x.parameter.ParameterName)
                                .BindCast<IRubyBlock, ITypedRubyBlock>()
                                .Map<ITypedRubyBlock, (RubyFunctionParameter parameter, ITypedRubyBlock rubyBlock)>(rubyBlock => (x.parameter, rubyBlock))

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
}