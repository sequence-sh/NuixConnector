using System;
using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.General;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal sealed class CompareIntConverter : CompareConverter<int> {}
    internal sealed class CompareStringConverter : CompareConverter<string> {}
    internal sealed class CompareBoolConverter : CompareConverter<bool> {}

    internal abstract class CompareConverter<T> : CoreTypedMethodConverter<Compare<T>, bool> where T : IComparable
    {
        /// <inheritdoc />
        protected override IEnumerable<(RubyFunctionParameter parameter, IRunnableProcess argumentProcess)> GetArgumentBlocks(Compare<T> process)
        {
            yield return (LeftParameter, process.Left);
            yield return (RightParameter, process.Right);
            yield return (OperatorParameter, process.Operator);
        }

        /// <inheritdoc />
        public override string FunctionName => "Compare";

        /// <inheritdoc />
        public override string FunctionText { get; } =
            $@"
    case {OperatorParameter.ParameterName}
    when '{CompareOperator.Equals.GetDisplayName()}'
        return {LeftParameter.ParameterName} == {RightParameter.ParameterName}
    when '{CompareOperator.GreaterThan.GetDisplayName()}'
        return {LeftParameter.ParameterName} > {RightParameter.ParameterName}
    when '{CompareOperator.LessThan.GetDisplayName()}'
        return {LeftParameter.ParameterName} < {RightParameter.ParameterName}
    when '{CompareOperator.GreaterThanOrEqual.GetDisplayName()}'
        return {LeftParameter.ParameterName} >= {RightParameter.ParameterName}
    when '{CompareOperator.LessThanOrEqual.GetDisplayName()}'
        return {LeftParameter.ParameterName} <= {RightParameter.ParameterName}
    when '{CompareOperator.NotEquals.GetDisplayName()}'
        return {LeftParameter.ParameterName} != {RightParameter.ParameterName}
    else
        raise '{OperatorParameter.ParameterName} not recognized'
    end
";

        public static readonly RubyFunctionParameter LeftParameter = new RubyFunctionParameter("leftArg", false);
        public static readonly RubyFunctionParameter OperatorParameter = new RubyFunctionParameter("operatorArg", false);
        public static readonly RubyFunctionParameter RightParameter = new RubyFunctionParameter("rightArg", false);

        /// <inheritdoc />
        public override IReadOnlyCollection<RubyFunctionParameter> Arguments { get; } =
            new[] {LeftParameter, OperatorParameter, RightParameter};
    }
}