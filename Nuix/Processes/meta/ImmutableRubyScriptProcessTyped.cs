using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{

    internal sealed class ImmutableRubyScriptProcessString : ImmutableRubyScriptProcessTyped<string>
    {
        /// <inheritdoc />
        public ImmutableRubyScriptProcessString(string name, IMethodCall<string> methodCall, INuixProcessSettings nuixProcessSettings) : base(name, methodCall, nuixProcessSettings)
        {
        }

        /// <inheritdoc />
        protected override bool TryParseResult(string r, out string value)
        {
            value = r;
            return !string.IsNullOrWhiteSpace(value);
        }
    }

    internal sealed class ImmutableRubyScriptProcessBool : ImmutableRubyScriptProcessTyped<bool>
    {
        /// <inheritdoc />
        public ImmutableRubyScriptProcessBool(string name, IMethodCall<bool> methodCall, INuixProcessSettings nuixProcessSettings) : base(name, methodCall, nuixProcessSettings)
        {
        }

        /// <inheritdoc />
        protected override bool TryParseResult(string r, out bool value)
        {
            return bool.TryParse(r, out value);
        }
    }

    internal sealed class ImmutableRubyScriptProcessInt : ImmutableRubyScriptProcessTyped<int>
    {
        /// <inheritdoc />
        public ImmutableRubyScriptProcessInt(string name, IMethodCall<int> methodCall, INuixProcessSettings nuixProcessSettings) : base(name, methodCall, nuixProcessSettings)
        {
        }

        /// <inheritdoc />
        protected override bool TryParseResult(string r, out int value)
        {
            return int.TryParse(r, out value);
        }
    }

    internal abstract class ImmutableRubyScriptProcessTyped<T> : ImmutableProcess<T>
    {
        private readonly IMethodCall<T> _methodCall;

        private readonly INuixProcessSettings _nuixProcessSettings;

        /// <inheritdoc />
        protected ImmutableRubyScriptProcessTyped(string name, IMethodCall<T> methodCall, INuixProcessSettings nuixProcessSettings) : base(name)
        {
            _methodCall = methodCall;
            _nuixProcessSettings = nuixProcessSettings;
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<T>> Execute()
        {
            var scriptText = CompileScript();
            var trueArguments = await RubyScriptCompilationHelper.GetTrueArgumentsAsync(scriptText, _nuixProcessSettings, new []{_methodCall});

            IProcessOutput<T>? successOutput = null;

            await foreach (var output in ExternalProcessHelper.RunExternalProcess(_nuixProcessSettings.NuixExeConsolePath, trueArguments))
            {
                if (output.OutputType == OutputType.Message && TryExtractValueFromOutput(output.Text, out var val))
                    successOutput = ProcessOutput<T>.Success(val);
                else
                    yield return output.ConvertTo<T>();
            }

            if (successOutput != null)
                yield return successOutput;
            else
                yield return ProcessOutput<T>.Error("Process did not complete successfully");
        }

        private string CompileScript()
        {
            var scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptSetup(Name, new []{_methodCall}));
            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptMethodText(new []{_methodCall}));


            var fullMethodLine = $"finalResult = {_methodCall}";

            scriptBuilder.AppendLine(fullMethodLine);

            scriptBuilder.AppendLine("puts \"--Final Result: #{finalResult}\"");

            return (scriptBuilder.ToString());
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly Regex FinalResultRegex = new Regex("--Final Result: (?<result>.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private bool TryExtractValueFromOutput(string message, out T value)
        {
            if (FinalResultRegex.TryMatch(message, out var m))
            {
                var resultString = m.Groups["result"].Value;

                return TryParseResult(resultString, out value);
            }

#pragma warning disable CS8601 // Possible null reference assignment.
            value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
            return false;
        }

        protected abstract bool TryParseResult(string r, out T value);

    }
}