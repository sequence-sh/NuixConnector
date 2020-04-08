using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    internal sealed class ImmutableRubyScriptProcessTyped<T> : ImmutableProcess<T>, IImmutableRubyScriptProcess
    {
        public readonly ITypedRubyBlock<T> RubyBlock;
        private readonly INuixProcessSettings _nuixProcessSettings;
        public readonly Func<string, Result<T>> TryParseFunc;

        /// <inheritdoc />
        public ImmutableRubyScriptProcessTyped( ITypedRubyBlock<T> rubyBlock, INuixProcessSettings nuixProcessSettings, Func<string, Result<T>> tryParseFunc) 
            
        {
            RubyBlock = rubyBlock;
            _nuixProcessSettings = nuixProcessSettings;
            TryParseFunc = tryParseFunc;
        }

        /// <inheritdoc />
        public override string Name => RubyBlock.BlockName;

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<T>> Execute()
        {
            var scriptText = CompileScript();
            var trueArguments = await RubyScriptCompilationHelper.GetTrueArgumentsAsync(scriptText, _nuixProcessSettings, new []{RubyBlock});

            IProcessOutput<T>? successOutput = null;

            await foreach (var output in ExternalProcessHelper.RunExternalProcess(_nuixProcessSettings.NuixExeConsolePath, trueArguments))
            {
                if (output.OutputType == OutputType.Message && TryExtractValueFromOutput(output.Text, out var val))
                    successOutput = ProcessOutput<T>.Success(val);
                else if (output.OutputType == OutputType.Error && RubyScriptCompilationHelper.NuixWarnings.Contains(output.Text))
                {
                    yield return ProcessOutput<T>.Warning(output.Text);
                }
                else
                    yield return output.ConvertTo<T>();
            }

            if (successOutput != null)
                yield return successOutput;
            else
                yield return ProcessOutput<T>.Error("Process did not complete successfully");
        }

        public string CompileScript()
        {
            var scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptSetup(Name, new []{RubyBlock}));
            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptMethodText(new []{RubyBlock}));

            var i = 0;
            var fullMethodLine = RubyBlock.GetBlockText(ref i, out var resultVariableName);

            scriptBuilder.AppendLine(fullMethodLine);
            scriptBuilder.AppendLine($"puts \"--Final Result: #{{{resultVariableName}}}\"");

            return (scriptBuilder.ToString());
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly Regex FinalResultRegex = new Regex("--Final Result: (?<result>.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private bool TryExtractValueFromOutput(string message, out T value)
        {
            if (FinalResultRegex.TryMatch(message, out var m))
            {
                var resultString = m.Groups["result"].Value;

                var (isSuccess, _, value1) = TryParseFunc(resultString);
                if (isSuccess)
                {
                    value = value1;
                    return true;
                }
            }

#pragma warning disable CS8601 // Possible null reference assignment.
            value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
            return false;
        }


        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => NuixProcessConverter.Instance;
    }
}