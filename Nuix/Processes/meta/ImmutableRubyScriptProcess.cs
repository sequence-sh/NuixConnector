using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    internal sealed class ImmutableRubyScriptProcess : ImmutableProcess<Unit>
    {
        /// <inheritdoc />
        public ImmutableRubyScriptProcess(
            INuixProcessSettings nuixProcessSettings,
            IReadOnlyCollection<IMethodCall<Unit>> methodCalls)
        {
            _nuixProcessSettings = nuixProcessSettings;
            _methodCalls = methodCalls;
        }

        private readonly INuixProcessSettings _nuixProcessSettings;


        private readonly IReadOnlyCollection<IMethodCall<Unit>> _methodCalls;

        private string CompileScript()
        {
            var scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptSetup(Name, _methodCalls));
            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptMethodText(_methodCalls));

            foreach (var line in _methodCalls.Select((x, i) => x.GetMethodLine(i)))
                scriptBuilder.AppendLine(line);


            scriptBuilder.AppendLine($"puts '{SuccessToken}'");

            return (scriptBuilder.ToString());
        }

        public const string SuccessToken = "--Script Completed Successfully--";


        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            var scriptText = CompileScript();
            var trueArguments = await RubyScriptCompilationHelper.GetTrueArgumentsAsync(scriptText, _nuixProcessSettings, _methodCalls);

            var succeeded = false;

            await foreach (var output in ExternalProcessHelper.RunExternalProcess(_nuixProcessSettings.NuixExeConsolePath, trueArguments))
            {
                if (output.OutputType == OutputType.Message && output.Text == SuccessToken)
                    succeeded = true;
                else
                    yield return output;
            }

            if (!succeeded)
                yield return ProcessOutput<Unit>.Error("Process did not complete successfully");
        }

        /// <inheritdoc />
        public override Result<ImmutableProcess<Unit>> TryCombine(ImmutableProcess<Unit> nextProcess, IProcessSettings processSettings)
        {
            var np = nextProcess as ImmutableRubyScriptProcess;
            if (np == null)
            {
                var (isSuccess, _, value) = NuixProcessConverter.Instance.TryConvert(nextProcess, processSettings);

                if (isSuccess)
                    np = value as ImmutableRubyScriptProcess;
            }

            if (np == null ||  !NuixProcessSettingsComparer.Instance.Equals(_nuixProcessSettings, np._nuixProcessSettings))
                return Result.Failure<ImmutableProcess<Unit>>("Could not combine");

            
            var newProcess = new ImmutableRubyScriptProcess(
                _nuixProcessSettings,
                _methodCalls.Concat(np._methodCalls).ToList());

            return Result.Success<ImmutableProcess<Unit>>(newProcess);
        }


        /// <inheritdoc />
        public override int GetHashCode()
        {
            return System.HashCode.Combine(Name);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (!(obj is ImmutableRubyScriptProcess rsp))
                return false;

            return Name == rsp.Name &&
                   NuixProcessSettingsComparer.Instance.Equals(_nuixProcessSettings, rsp._nuixProcessSettings)
                    &&
                   _methodCalls.SequenceEqual(rsp._methodCalls);
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetSequenceName(_methodCalls.Select(x => x.MethodName));

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => NuixProcessConverter.Instance;
    }

}