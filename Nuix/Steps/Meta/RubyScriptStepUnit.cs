using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    /// <summary>
    /// The base of ruby script steps
    /// </summary>
    public abstract class RubyScriptStepUnit : RubyScriptStepBase<Unit>
    {
        /// <summary>
        /// Gets the ruby block to run.
        /// </summary>
        private Result<IUnitRubyBlock, IRunErrors> TryGetRubyBlock(StateMonad stateMonad) =>
            TryGetMethodParameters(stateMonad)
                .Map(x =>
                    new UnitFunctionRubyBlock(RubyScriptStepFactory.RubyFunction,x ) as IUnitRubyBlock);


        /// <inheritdoc />
        public override Result<string, IRunErrors> TryCompileScript(StateMonad stateMonad) => TryGetRubyBlock(stateMonad)
            .Bind(ScriptGenerator.CompileScript);


        /// <inheritdoc />
        public override Result<IRubyBlock> TryConvert() =>
            TryGetArgumentsAsFunctions()
                .Map(args=> new UnitCompoundRubyBlock(RubyScriptStepFactory.RubyFunction, args) as IRubyBlock);


        /// <summary>
        /// Runs this step asynchronously.
        /// There are two possible strategies:
        /// Either running all as one ruby block;
        /// Or running all the parameters separately and passing them to the the final ruby block.
        /// </summary>
        protected override async Task<Result<Unit, IRunErrors>> RunAsync(StateMonad stateMonad)
        {
            var settingsResult = stateMonad.GetSettings<INuixSettings>(Name);
            if (settingsResult.IsFailure)
                return settingsResult.ConvertFailure<Unit>();


            IUnitRubyBlock block;


            var argsAsFunctionsResult = TryGetArgumentsAsFunctions();
            if (argsAsFunctionsResult.IsSuccess)
            {
                block = new UnitCompoundRubyBlock(RubyScriptStepFactory.RubyFunction, argsAsFunctionsResult.Value);
            }
            else
            {
                var blockResult = TryGetRubyBlock(stateMonad);//This will run child functions
                if (blockResult.IsFailure)
                    return blockResult.ConvertFailure<Unit>();
                block = blockResult.Value;

            }

            var r = await RubyBlockRunner.RunAsync(FunctionName, block, stateMonad, settingsResult.Value);

            return r;
        }
    }

}