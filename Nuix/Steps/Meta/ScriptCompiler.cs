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
        /// Compiles the text of a function
        /// </summary>
        public static string CompileFunctionText(this IRubyFunction function)
        {
            var stringBuilder = new StringBuilder();


            var methodHeader = $@"def {function.FunctionName}(args)";
            

            stringBuilder.AppendLine(methodHeader);

            foreach (var parameter in function.Arguments)
            {
                stringBuilder.AppendLine($"{parameter.ParameterName} = args['{parameter.ParameterName}']");
            }

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
