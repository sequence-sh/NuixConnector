using System;
using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.General;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Conversion
{
    internal sealed class CompareIntConverter : CompareConverter<int>
    {
        /// <inheritdoc />
        protected override string ConvertFuncName => "to_i";
    }

    internal sealed class CompareStringConverter : CompareConverter<string>
    {
        /// <inheritdoc />
        protected override string ConvertFuncName => "to_s";
    }

    internal sealed class CompareBoolConverter : CompareConverter<bool>
    {
        /// <inheritdoc />
        protected override string ConvertFuncName => "to_s.downcase";
    }

    internal abstract class CompareConverter<T> : CoreTypedMethodConverter<Compare<T>, bool> where T : IComparable
    {
        /// <inheritdoc />
        protected override IEnumerable<(RubyFunctionParameter parameter, IStep argumentProcess)> GetArgumentBlocks(Compare<T> process)
        {
            yield return (LeftParameter, process.Left);
            yield return (RightParameter, process.Right);
            yield return (OperatorParameter, process.Operator);
        }

        /// <inheritdoc />
        public override string FunctionName => "Compare";

        protected abstract string ConvertFuncName { get; }

        /// <inheritdoc />
        public override string FunctionText =>
            $@"
    puts ""#{{{LeftParameter.ParameterName}}} #{{{OperatorParameter.ParameterName}}} #{{{RightParameter.ParameterName}}}""

    case {OperatorParameter.ParameterName}
    when '{CompareOperator.Equals.GetDisplayName()}'
        return {LeftParameter.ParameterName}.{ConvertFuncName} == {RightParameter.ParameterName}.{ConvertFuncName}
    when '{CompareOperator.GreaterThan.GetDisplayName()}'
        return {LeftParameter.ParameterName}.{ConvertFuncName} > {RightParameter.ParameterName}.{ConvertFuncName}
    when '{CompareOperator.LessThan.GetDisplayName()}'
        return {LeftParameter.ParameterName}.{ConvertFuncName} < {RightParameter.ParameterName}.{ConvertFuncName}
    when '{CompareOperator.GreaterThanOrEqual.GetDisplayName()}'
        return {LeftParameter.ParameterName}.{ConvertFuncName} >= {RightParameter.ParameterName}.{ConvertFuncName}
    when '{CompareOperator.LessThanOrEqual.GetDisplayName()}'
        return {LeftParameter.ParameterName}.{ConvertFuncName} <= {RightParameter.ParameterName}.{ConvertFuncName}
    when '{CompareOperator.NotEquals.GetDisplayName()}'
        return {LeftParameter.ParameterName}.{ConvertFuncName} != {RightParameter.ParameterName}.{ConvertFuncName}
    else
        raise '{OperatorParameter.ParameterName} not recognized'
    end
";

        public static readonly RubyFunctionParameter LeftParameter = new RubyFunctionParameter("leftArg", false, null);
        public static readonly RubyFunctionParameter OperatorParameter = new RubyFunctionParameter("operatorArg", false, null);
        public static readonly RubyFunctionParameter RightParameter = new RubyFunctionParameter("rightArg", false, null);

        /// <inheritdoc />
        public override IReadOnlyCollection<RubyFunctionParameter> Arguments { get; } =
            new[] {LeftParameter, OperatorParameter, RightParameter};
    }
}