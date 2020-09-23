//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;

//namespace Reductech.EDR.Connectors.Nuix.Processes.Meta
//{
//    /// <summary>
//    /// A ruby block representing a single method call with no return value.
//    /// </summary>
//    internal sealed class BasicRubyBlock : AbstractBasicRubyBlock, IUnitRubyBlock
//    {
//        /// <inheritdoc />
//        public BasicRubyBlock(string blockName,
//            string functionText,
//            IReadOnlyCollection<RubyMethodParameter> methodParameters,
//            Version requiredNuixVersion,
//            IReadOnlyCollection<NuixFeature> requiredNuixFeatures) :
//            base(blockName, functionText, methodParameters, true)
//        {
//            RequiredNuixVersion = requiredNuixVersion;
//            RequiredNuixFeatures = requiredNuixFeatures;
//        }

//        /// <inheritdoc />
//        public string GetBlockText(ref int blockNumber)
//        {
//            var args = new List<string>();

//            foreach (var (argumentName, _, _) in MethodParameters)
//            {
//                var newParameterName = argumentName + blockNumber;
//                args.Add($"{RubyScriptCompilationHelper.HashSetName}[:{newParameterName}]");
//            }

//            var r = GetBlockText(ref blockNumber, args);

//            return r;
//        }

//        /// <inheritdoc />
//        public string GetBlockText(ref int blockNumber, IReadOnlyCollection<string> arguments)
//        {
//            //BlockName is also the name of the method
//            var callStringBuilder = new StringBuilder(BlockName + "(");


//            callStringBuilder.AppendJoin(", ", arguments.Prepend(UtilitiesParameterName));

//            callStringBuilder.Append(")");
//            blockNumber++;
//            return (callStringBuilder.ToString());
//        }

//        /// <inheritdoc />
//        public override Version RequiredNuixVersion { get; }

//        /// <inheritdoc />
//        public override IReadOnlyCollection<NuixFeature> RequiredNuixFeatures { get; }
//    }
//}