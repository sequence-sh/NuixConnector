//using System.Collections.Generic;
//using System.Linq;
//using CSharpFunctionalExtensions;
//using Reductech.EDR.Core.Internal;
//using Reductech.EDR.Core.Internal.Errors;

//namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
//{
//    /// <summary>
//    /// A ruby block that executes a function where every child function is another block.
//    /// </summary>
//    public abstract class CompoundRubyBlock : IRubyBlock
//    {
//        /// <summary>
//        /// Creates a new CompoundRubyBlock
//        /// </summary>
//        protected CompoundRubyBlock(
//            IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> arguments) =>
//            Arguments = arguments;

//        /// <summary>
//        /// The final function to run.
//        /// </summary>
//        public abstract IRubyFunction Function { get; }

//        /// <summary>
//        /// Arguments to the function in the form of ruby blocks.
//        /// </summary>
//        public IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> Arguments { get; }

//        /// <inheritdoc />
//        public string Name => Function.FunctionName;

//        /// <inheritdoc />
//        public override string ToString() => Name;

//        /// <inheritdoc />
//        public IEnumerable<IRubyFunction> FunctionDefinitions =>
//            Arguments.SelectMany(x => x.Value.FunctionDefinitions).Append(Function);

//        /// <inheritdoc />
//        public Result<IReadOnlyCollection<string>, IErrorBuilder> TryGetArguments(Suffixer suffixer)
//        {
//            var resultsStrings = new List<string>();
//            var errors = new List<IErrorBuilder>();

//            var childSuffixer = suffixer.GetNextChild();

//            foreach (var rubyFunctionArgument in Function.Arguments)
//            {

//                if (Arguments.TryGetValue(rubyFunctionArgument, out var block))
//                {
//                    var childResult = block.TryGetArguments(childSuffixer);

//                    if (childResult.IsSuccess)
//                        resultsStrings.AddRange(childResult.Value);
//                    else
//                        errors.Add(childResult.Error);
//                }
//                else
//                {
//                    childSuffixer.GetNext();
//                    if (!rubyFunctionArgument.IsOptional)
//                        errors.Add(ErrorHelper.MissingParameterError(rubyFunctionArgument.ParameterName,
//                            Function.FunctionName));

//                    //else do nothing, though note that the suffixer is incremented
//                }
//            }

//            if (errors.Any())
//                ErrorBuilderList.Combine(errors);

//            return resultsStrings;
//        }

//        /// <inheritdoc />
//        public void WriteOptParseLines(string hashSetName, IIndentationStringBuilder sb, Suffixer suffixer)
//        {
//            var childSuffixer = suffixer.GetNextChild();
//            foreach (var rubyFunctionArgument in Function.Arguments)
//            {
//                if (Arguments.TryGetValue(rubyFunctionArgument, out var block))
//                    block.WriteOptParseLines(hashSetName, sb, childSuffixer);
//                //else assume the argument was optional, it will be nil later
//            }

//        }
//    }
//}


