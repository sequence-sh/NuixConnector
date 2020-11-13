using System;
using System.Linq;
using System.Text;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    /// <summary>
    /// Compiles scripts
    /// </summary>
    public static class ScriptCompiler
    {
        /// <summary>
        /// The name of the nuix utilities class
        /// </summary>
        public const string UtilitiesParameterName = "utilities";

        /// <summary>
        /// Compiles the text of a function
        /// </summary>
        public static string CompileFunctionText(this IRubyFunction function)
        {
            var stringBuilder = new StringBuilder();

            var parameters = function.Arguments.Select(x => x.ParameterName);


            if (function.RequireUtilities)
                parameters = parameters.Prepend(UtilitiesParameterName);


            var methodHeader = $@"def {function.FunctionName}(args)";

            //{string.Join(",", parameters)}
            foreach (var parameter in function.Arguments)
            {
                stringBuilder.AppendLine($"parameter = args[\"{parameter}\"]");
            }


            stringBuilder.AppendLine(methodHeader);
            stringBuilder.AppendLine(IndentFunctionText(function.FunctionText));
            stringBuilder.AppendLine("end");
            stringBuilder.AppendLine();

            return stringBuilder.ToString();

            static string IndentFunctionText(string functionText)
            {
                functionText = functionText.Trim('\n', '\r');

                if (functionText.StartsWith('\t') || functionText.StartsWith(' '))
                    return functionText;

                var indentedText = string.Join(Environment.NewLine,
                    functionText.Split(Environment.NewLine).Select(x => '\t' + x));

                return indentedText;
            }
        }

    }
}
