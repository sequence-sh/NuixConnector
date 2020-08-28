using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// A ruby block that executes a function where every child function is another block.
    /// </summary>
    public abstract class CompoundRubyBlock : IRubyBlock
    {
        /// <summary>
        /// The final function to run.
        /// </summary>
        public abstract IRubyFunction Function { get; }

        /// <summary>
        /// Arguments to the function in the form of ruby blocks.
        /// </summary>
        public IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> Arguments { get; }

        /// <summary>
        /// Creates a new CompoundRubyBlock
        /// </summary>
        protected CompoundRubyBlock(
            IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> arguments) =>
            Arguments = arguments;


        /// <inheritdoc />
        public IEnumerable<IRubyFunction> FunctionDefinitions =>
            Arguments.SelectMany(x => x.Value.FunctionDefinitions).Append(Function);

        /// <inheritdoc />
        public Result<IReadOnlyCollection<string>, IRunErrors> TryGetArguments(Suffixer suffixer)
        {
            var resultsStrings = new List<string>();
            var errors = new List<IRunErrors>();

            foreach (var rubyFunctionArgument in Function.Arguments)
            {
                var childSuffixer = suffixer.GetNextChild();
                if (Arguments.TryGetValue(rubyFunctionArgument, out var block))
                {
                    var childResult = block.TryGetArguments(childSuffixer);

                    if (childResult.IsSuccess)
                        resultsStrings.AddRange(childResult.Value);
                    else
                        errors.Add(childResult.Error);
                }
                else if (!rubyFunctionArgument.IsOptional)
                    errors.Add(ErrorHelper.MissingParameterError(rubyFunctionArgument.ParameterName,
                        Function.FunctionName));

                //else do nothing, though note that the suffixer is incremented
            }

            if (errors.Any())
                return Result.Failure<IReadOnlyCollection<string>, IRunErrors>(RunErrorList.Combine(errors));

            return resultsStrings;
        }

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetOptParseLines(string hashSetName, Suffixer suffixer)
        {
            var r = new List<string>();

            foreach (var rubyFunctionArgument in Function.Arguments)
            {
                var childSuffixer = suffixer.GetNextChild();
                if (Arguments.TryGetValue(rubyFunctionArgument, out var block))
                    r.AddRange(block.GetOptParseLines(hashSetName, childSuffixer));
                //else assume the argument was optional, it will be nil later
            }

            return r;
        }

        internal Result<IReadOnlyCollection<string>, IRunErrors> GetArguments(Suffixer suffixer)
        {
            var argumentValues = new List<string>();
            var errors = new List<IRunErrors>();

            foreach (var rubyFunctionArgument in Function.Arguments)
            {
                var childSuffixer = suffixer.GetNextChild();
                if (Arguments.TryGetValue(rubyFunctionArgument, out var block))
                {
                    block.GetBlockText(childSuffixer, out var variableName);
                    argumentValues.Add(variableName);
                }
                else if (rubyFunctionArgument.IsOptional)
                    argumentValues.Add("nil");
                else
                    errors.Add(ErrorHelper.MissingParameterError(rubyFunctionArgument.ParameterName,
                        Function.FunctionName));
            }

            if (errors.Any())
                return Result.Failure<IReadOnlyCollection<string>, IRunErrors>(RunErrorList.Combine(errors));

            return argumentValues;
        }


    }

    /// <summary>
    /// A ruby block where all the arguments are ruby functions.
    /// </summary>
    public sealed class TypedCompoundRubyBlock<T> : CompoundRubyBlock, ITypedRubyBlock<T>
    {
        /// <inheritdoc />
        public TypedCompoundRubyBlock(IRubyFunction<T> typedFunction, IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> arguments) : base(arguments)
        {
            TypedFunction = typedFunction;
        }

        /// <summary>
        /// The final function to run.
        /// </summary>
        public IRubyFunction<T> TypedFunction { get; }

        /// <inheritdoc />
        public override IRubyFunction Function => TypedFunction;

        /// <inheritdoc />
        public Result<string, IRunErrors> GetBlockText(Suffixer suffixer, out string resultVariableName)
        {
            var stringBuilder = new StringBuilder();

            resultVariableName = Function.FunctionName + suffixer.CurrentSuffix;

            var argumentResult = GetArguments(suffixer);

            if (argumentResult.IsFailure)
                return argumentResult.ConvertFailure<string>();

            stringBuilder.Append(Function.FunctionName);
            stringBuilder.Append($"{resultVariableName} = (");
            stringBuilder.AppendJoin(", ", argumentResult.Value);
            stringBuilder.Append(")");

            return stringBuilder.ToString();
        }
    }

    /// <summary>
    /// A ruby block where all the arguments are ruby functions.
    /// </summary>
    public sealed class UnitCompoundRubyBlock : CompoundRubyBlock, IUnitRubyBlock
    {
        /// <inheritdoc />
        public UnitCompoundRubyBlock(IRubyFunction<Unit> unitFunction, IReadOnlyDictionary<RubyFunctionParameter, ITypedRubyBlock> arguments) : base(arguments)
        {
            UnitFunction = unitFunction;
        }

        /// <summary>
        /// The final function to run.
        /// </summary>
        public IRubyFunction<Unit> UnitFunction { get; }

        /// <inheritdoc />
        public override IRubyFunction Function => UnitFunction;

        /// <inheritdoc />
        public Result<string, IRunErrors> GetBlockText(Suffixer suffixer)
        {
            var stringBuilder = new StringBuilder();

            var argumentResult = GetArguments(suffixer);
            if (argumentResult.IsFailure) return argumentResult.ConvertFailure<string>();

            stringBuilder.Append(Function.FunctionName);
            stringBuilder.Append("(");
            stringBuilder.AppendJoin(", ", argumentResult.Value);
            stringBuilder.Append(")");

            return stringBuilder.ToString();

        }
    }



    ///// <summary>
    ///// A sequence of ruby blocks with no final output.
    ///// </summary>
    //public class CompoundRubyBlock : IUnitRubyBlock
    //{
    //    /// <summary>
    //    /// Create a new compound ruby block.
    //    /// </summary>
    //    public CompoundRubyBlock(IUnitRubyBlock finalBlock, IReadOnlyList<ITypedRubyBlock> parameterBlocks)
    //    {
    //        FinalBlock = finalBlock;
    //        ParameterBlocks = parameterBlocks;
    //    }


    //    /// <summary>
    //    /// The block that will actually be run.
    //    /// </summary>
    //    public IUnitRubyBlock FinalBlock { get; }

    //    /// <summary>
    //    /// Blocks that make up this block.
    //    /// </summary>
    //    public IReadOnlyList<ITypedRubyBlock> ParameterBlocks { get; }


    //    /// <inheritdoc />
    //    public string BlockName => string.Join(' ', ParameterBlocks.Select(x => x.BlockName));

    //    /// <inheritdoc />
    //    public Version RequiredNuixVersion => ParameterBlocks.Select(x => x.RequiredNuixVersion).Prepend(FinalBlock.RequiredNuixVersion).Max();

    //    /// <inheritdoc />
    //    public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures => ParameterBlocks.SelectMany(x=>x.RequiredNuixFeatures).Concat(FinalBlock.RequiredNuixFeatures).ToHashSet();

    //    /// <inheritdoc />
    //    public IEnumerable<string> FunctionDefinitions => ParameterBlocks.SelectMany(x => x.FunctionDefinitions).Concat(FinalBlock.FunctionDefinitions).ToHashSet();

    //    /// <inheritdoc />
    //    public IReadOnlyCollection<string> GetArguments(ref int blockNumber)
    //    {
    //        var allArguments = new List<string>();

    //        foreach (var rubyBlock in ParameterBlocks)
    //            allArguments.AddRange(rubyBlock.GetArguments(ref blockNumber));

    //        return allArguments;
    //    }

    //    /// <inheritdoc />
    //    public IReadOnlyCollection<string> GetOptParseLines(string hashSetName, ref int blockNumber)
    //    {
    //        var lines = new List<string>();

    //        foreach (var rubyBlock in ParameterBlocks)
    //            lines.AddRange(rubyBlock.GetOptParseLines(hashSetName, ref blockNumber));

    //        return lines;
    //    }

    //    /// <inheritdoc />
    //    public string GetBlockText(ref int blockNumber)
    //    {
    //        var sb = new StringBuilder();
    //        var variables = new List<string>();

    //        foreach (var rubyBlock in ParameterBlocks)
    //        {
    //            sb.AppendLine(rubyBlock.GetBlockText(ref blockNumber, out var variableName));
    //            variables.Add(variableName);
    //        }


    //    }
    //}
}