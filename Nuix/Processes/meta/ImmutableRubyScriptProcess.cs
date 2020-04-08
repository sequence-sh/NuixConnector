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
    internal sealed class ImmutableRubyScriptProcess : ImmutableProcess<Unit>, IImmutableRubyScriptProcess
    {
        /// <inheritdoc />
        public ImmutableRubyScriptProcess(IReadOnlyCollection<IUnitRubyBlock> rubyBlocks,
            INuixProcessSettings nuixProcessSettings)
        {
            _nuixProcessSettings = nuixProcessSettings;
            RubyBlocks = rubyBlocks;
        }

        private readonly INuixProcessSettings _nuixProcessSettings;


        public readonly IReadOnlyCollection<IUnitRubyBlock> RubyBlocks;

        public string CompileScript()
        {
            var scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptSetup(Name, RubyBlocks));
            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptMethodText(RubyBlocks));

            var i = 0;
            foreach (var rubyBlock in RubyBlocks)
            {
                var blockText = rubyBlock.GetBlockText(ref i);
                scriptBuilder.AppendLine(blockText);
            }

            scriptBuilder.AppendLine($"puts '{SuccessToken}'");

            return (scriptBuilder.ToString());
        }

        public const string SuccessToken = "--Script Completed Successfully--";


        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            if (RubyBlocks.Any())
            {
                var scriptText = CompileScript();
                var trueArguments = await RubyScriptCompilationHelper.GetTrueArgumentsAsync(scriptText, _nuixProcessSettings, RubyBlocks);

                var succeeded = false;

                await foreach (var output in ExternalProcessHelper.RunExternalProcess(_nuixProcessSettings.NuixExeConsolePath, trueArguments))
                {
                    if (output.OutputType == OutputType.Message && output.Text == SuccessToken)
                        succeeded = true;
                    else if(output.OutputType == OutputType.Error && RubyScriptCompilationHelper.NuixWarnings.Contains(output.Text))
                    {
                        yield return ProcessOutput<Unit>.Warning(output.Text);
                    }
                    else
                        yield return output;
                }

                if (!succeeded)
                    yield return ProcessOutput<Unit>.Error("Process did not complete successfully");
            }
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

            if (np == null || !(processSettings is INuixProcessSettings iNuixProcessSettings))
                return Result.Failure<ImmutableProcess<Unit>>("Could not combine");

            
            var newProcess = new ImmutableRubyScriptProcess(RubyBlocks.Concat(np.RubyBlocks).ToList(), iNuixProcessSettings);

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

            return Name == rsp.Name && RubyBlocks.SequenceEqual(rsp.RubyBlocks);
        }

        /// <inheritdoc />
        public override string Name => ProcessNameHelper.GetSequenceName(RubyBlocks.Select(x => x.BlockName));

        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => NuixProcessConverter.Instance;
    }

}