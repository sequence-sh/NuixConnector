using System;
using System.Collections.Generic;
using System.Text;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// A ruby block representing a single method call with a return value of a particular type.
    /// </summary>
    internal sealed class BasicTypedRubyBlock<T> : AbstractBasicRubyBlock, ITypedRubyBlock<T>
    {
        /// <inheritdoc />
        public BasicTypedRubyBlock(string blockName, string functionText, IReadOnlyCollection<RubyMethodParameter> methodParameters, Version requiredNuixVersion, IReadOnlyCollection<NuixFeature> requiredNuixFeatures)
            : base(blockName, functionText, methodParameters)
        {
            RequiredNuixVersion = requiredNuixVersion;
            RequiredNuixFeatures = requiredNuixFeatures;
        }

        /// <inheritdoc />
        public string GetBlockText(ref int blockNumber, out string resultVariableName)
        {
            resultVariableName = "result" + blockNumber;
            //BlockName is also the name of the method
            var callStringBuilder = new StringBuilder($"{resultVariableName} = {BlockName}(utilities"); //utilities is always first argument

            foreach (var (argumentName, _, _) in MethodParameters)
            {
                var newParameterName = argumentName + blockNumber;
                callStringBuilder.Append(", ");

                callStringBuilder.Append($"{RubyScriptCompilationHelper.HashSetName}[:{newParameterName}]");
            }

            callStringBuilder.Append(")");
            blockNumber++;
            return callStringBuilder.ToString();
        }

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; }

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredNuixFeatures { get; }
    }
}