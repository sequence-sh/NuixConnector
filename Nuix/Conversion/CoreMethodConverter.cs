using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal abstract class CoreMethodConverter<TStep> : ICoreMethodConverter, IRubyFunction
    {
        /// <inheritdoc />
        public Result<IRubyBlock> Convert(IStep step)
        {
            if (step is TStep tStep)
            {
                var argumentValuesResult =
                    GetArgumentBlocks(tStep).Select(x =>
                            RubyBlockConversion.TryConvert(x.argumentProcess, x.parameter.ParameterName)
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

        protected abstract IEnumerable<(RubyFunctionParameter parameter, IStep argumentProcess)> GetArgumentBlocks(TStep process);


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