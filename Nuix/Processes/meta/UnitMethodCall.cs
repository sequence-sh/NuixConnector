using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    internal sealed class BasicMethodCall<T> : IMethodCall<T>
    {
        public string MethodName { get; }
        public string MethodText { get; }
        private readonly IReadOnlyCollection<(string argumentName, string? argumentValue, bool valueCanBeNull)> _methodParameters;

        public BasicMethodCall(string methodName, string methodText, IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> methodParameters)
        {
            MethodName = methodName;
            MethodText = methodText;
            _methodParameters = methodParameters.ToList();
        }

        public string GetMethodLine(int methodNumber)
        {
            var callStringBuilder = new StringBuilder(MethodName + "(utilities"); //utilities is always first argument

            foreach (var (argumentName, _, _) in _methodParameters)
            {
                var newParameterName = argumentName + methodNumber;
                callStringBuilder.Append(", ");

                callStringBuilder.Append($"{RubyScriptCompilationHelper.HashSetName}[:{newParameterName}]");
            }

            callStringBuilder.Append(")");

            return (callStringBuilder.ToString());
        }

        public IEnumerable<string> GetArguments(int methodNumber)
        {
            foreach (var (argumentName, argumentValue, _) in _methodParameters)
            {
                var newParameterName = argumentName + methodNumber;
                if (argumentValue == null) continue;

                yield return $"--{newParameterName}";
                yield return argumentValue;
            }
        }

        public IEnumerable<string> GetOptParseLines(int methodNumber)
        {
            var optParseLines = new List<string>();

            foreach (var (argumentName, _, valueCanBeNull) in _methodParameters)
            {
                var newParameterName = argumentName + methodNumber;
                var argTerm = valueCanBeNull ? "[ARG]" : "ARG";

                optParseLines.Add($"opts.on('--{newParameterName} {argTerm}')");
            }

            return optParseLines;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return MethodName;
        }
    }
}