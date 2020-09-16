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
        /// Gets the ruby block to run.
        /// </summary>
        private Result<IUnitRubyBlock, IRunErrors> TryGetRubyBlock(ProcessState processState) =>
            TryGetMethodParameters(processState)
                .Map(x =>
                    new UnitFunctionRubyBlock(RubyScriptProcessFactory.RubyFunction,x ) as IUnitRubyBlock);


        /// <inheritdoc />
        public override Result<string, IRunErrors> TryCompileScript(ProcessState processState) => TryGetRubyBlock(processState)
            .Bind(ScriptGenerator.CompileScript);


        /// <inheritdoc />
        public override Result<IRubyBlock> TryConvert() =>
            TryGetArgumentsAsFunctions()
                .Map(args=> new UnitCompoundRubyBlock(RubyScriptProcessFactory.RubyFunction, args) as IRubyBlock);


        /// <summary>
        /// Runs this process asynchronously.
        /// There are two possible strategies:
        /// Either running all as one ruby block;
        /// Or running all the parameters separately and passing them to the the final ruby block.
        /// </summary>
        protected override async Task<Result<Unit, IRunErrors>> RunAsync(ProcessState processState)
        {
            var settingsResult = processState.GetProcessSettings<INuixProcessSettings>(Name);
            if (settingsResult.IsFailure)
                return settingsResult.ConvertFailure<Unit>();


            IUnitRubyBlock block;


            var argsAsFunctionsResult = TryGetArgumentsAsFunctions();
            if (argsAsFunctionsResult.IsSuccess)
            {
                block = new UnitCompoundRubyBlock(RubyScriptProcessFactory.RubyFunction, argsAsFunctionsResult.Value);
            }
            else
            {
                var blockResult = TryGetRubyBlock(processState);//This will run child functions
                if (blockResult.IsFailure)
                    return blockResult.ConvertFailure<Unit>();
                block = blockResult.Value;

            }

            var r = await RubyProcessRunner.RunAsync(FunctionName, block, processState, settingsResult.Value);

            return r;
        }
    }

}