using System;
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
    public abstract class RubyScriptProcess : RubyScriptProcessBase<Unit>
    {
        /// <summary>
        /// The string to use for the Nuix requirement
        /// </summary>
        public const string NuixProcessName = "Nuix";

        /// <summary>
        /// The ruby blocks that make up this process.
        /// </summary>
        public IReadOnlyCollection<IUnitRubyBlock> RubyBlocks => new IUnitRubyBlock[]{
            new BasicRubyBlock(MethodName, ScriptText, MethodParameters, RunTimeNuixVersion?? RubyScriptProcessFactory.RequiredVersion, RubyScriptProcessFactory.RequiredFeatures), };

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


        /// <inheritdoc />
        public override string CompileScript()
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

        /// <summary>
        /// The string that will be returned when the script completes successfully.
        /// </summary>
        public const string SuccessToken = "--Script Completed Successfully--";


        /// <summary>
        /// Runs this process asynchronously
        /// </summary>
        protected override async Task<Result<Unit, IRunErrors>> RunAsync(ProcessState processState)
        {
            if (RubyBlocks.Any())
            {
                var settingsResult = processState.GetProcessSettings<INuixProcessSettings>(Name);
                if (settingsResult.IsFailure) return settingsResult.ConvertFailure<Unit>();

                var scriptText = CompileScript();
                var trueArguments = await RubyScriptCompilationHelper.GetTrueArgumentsAsync(scriptText, settingsResult.Value, RubyBlocks);


                var result =  await ExternalProcessMethods.RunExternalProcess(settingsResult.Value.NuixExeConsolePath, processState.Logger,
                    Name,
                    trueArguments);

                return result;
            }

            return Unit.Default;
        }

        //TODO restore combiners
        ///// <inheritdoc />
        //public override Result<IImmutableProcess<Unit>> TryCombine(IImmutableProcess<Unit> nextProcess, IProcessSettings processSettings)
        //{
        //    var np = nextProcess as RubyScriptProcess;
        //    if (np == null)
        //    {
        //        var (isSuccess, _, value) = NuixProcessConverter.Instance.TryConvert(nextProcess, processSettings);

        //        if (isSuccess)
        //            np = value as RubyScriptProcess;
        //    }

        //    if (np == null || !(processSettings is INuixProcessSettings iNuixProcessSettings))
        //        return Result.Failure<IImmutableProcess<Unit>>("Could not combine");

        //    var newProcess = new RubyScriptProcess(RubyBlocks.Concat(np.RubyBlocks).ToList(), iNuixProcessSettings);

        //    return newProcess;
        //}




    }

}