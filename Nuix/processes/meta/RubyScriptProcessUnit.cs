using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// The base of ruby script processes
    /// </summary>
    public abstract class RubyScriptProcessUnit : RubyScriptProcessBase<Unit>
    {
        /// <summary>
        /// The string to use for the Nuix requirement
        /// </summary>
        public const string NuixProcessName = "Nuix";

        /// <summary>
        /// Gets the ruby block to run.
        /// </summary>
        private Result<IUnitRubyBlock, IRunErrors> TryGetRubyBlock(ProcessState processState)
        {
            var parametersResult = TryGetMethodParameters(processState)
                .Combine(RunErrorList.Combine)
                .Map(x => x.ToList())
                .Map(x =>
                    new BasicRubyBlock(
                        MethodName,
                        ScriptText, x,
                        RunTimeNuixVersion ?? RubyScriptProcessFactory.RequiredVersion,
                RubyScriptProcessFactory.RequiredFeatures)  as IUnitRubyBlock);

            return parametersResult;

        }

        /// <inheritdoc />
        public override IEnumerable<Requirement> RuntimeRequirements
        {
            get
            {
                if (RunTimeNuixVersion != null)
                {
                    yield return new Requirement
                    {
                        MinVersion = RunTimeNuixVersion,
                        Name = NuixProcessName
                    };
                }
            }
        }


        private string CompileScript(IUnitRubyBlock block)
        {
            var scriptBuilder = new StringBuilder();
            var blocks = new[] {block};

            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptSetup(MethodName, blocks));
            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptMethodText(blocks));

            var i = 0;
            foreach (var rubyBlock in blocks)
            {
                var blockText = rubyBlock.GetBlockText(ref i);
                scriptBuilder.AppendLine(blockText);
            }

            scriptBuilder.AppendLine($"puts '{SuccessToken}'");

            return (scriptBuilder.ToString());
        }

        /// <inheritdoc />
        public override Result<string, IRunErrors> TryCompileScript(ProcessState processState) => TryGetRubyBlock(processState).Map(CompileScript);

        /// <summary>
        /// The string that will be returned when the script completes successfully.
        /// </summary>
        public const string SuccessToken = "--Script Completed Successfully--";


        /// <summary>
        /// Runs this process asynchronously
        /// </summary>
        protected override async Task<Result<Unit, IRunErrors>> RunAsync(ProcessState processState)
        {

            var blockResult = TryGetRubyBlock(processState);

            if (blockResult.IsFailure)
                return blockResult.ConvertFailure<Unit>();


            var settingsResult = processState.GetProcessSettings<INuixProcessSettings>(Name);
            if (settingsResult.IsFailure) return settingsResult.ConvertFailure<Unit>();

            var scriptText = CompileScript(blockResult.Value);
            var trueArguments = await RubyScriptCompilationHelper.GetTrueArgumentsAsync(scriptText, settingsResult.Value, new []{blockResult.Value});


            var result = await ExternalProcessMethods.RunExternalProcess(settingsResult.Value.NuixExeConsolePath, processState.Logger,
                Name,
                trueArguments);

            return result;
        }

        //TODO restore combiners
        ///// <inheritdoc />
        //public override Result<IImmutableProcess<Unit>> TryCombine(IImmutableProcess<Unit> nextProcess, IProcessSettings processSettings)
        //{
        //    var np = nextProcess as RubyScriptProcessUnit;
        //    if (np == null)
        //    {
        //        var (isSuccess, _, value) = NuixProcessConverter.Instance.TryConvert(nextProcess, processSettings);

        //        if (isSuccess)
        //            np = value as RubyScriptProcessUnit;
        //    }

        //    if (np == null || !(processSettings is INuixProcessSettings iNuixProcessSettings))
        //        return Result.Failure<IImmutableProcess<Unit>>("Could not combine");

        //    var newProcess = new RubyScriptProcessUnit(RubyBlocks.Concat(np.RubyBlocks).ToList(), iNuixProcessSettings);

        //    return newProcess;
        //}




    }

}