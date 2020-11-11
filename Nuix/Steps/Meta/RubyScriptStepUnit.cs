using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal.Errors;
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
        private Task<Result<IUnitRubyBlock, IError>>  TryGetRubyBlock(StateMonad stateMonad, CancellationToken cancellationToken) =>
            TryGetMethodParameters(stateMonad, cancellationToken)
                .Map(x =>
                    new UnitFunctionRubyBlock(RubyScriptStepFactory.RubyFunction,x ) as IUnitRubyBlock);


        /// <inheritdoc />
        public override Task<Result<string, IError>> TryCompileScriptAsync(StateMonad stateMonad, CancellationToken cancellationToken) =>
            TryGetRubyBlock(stateMonad, cancellationToken)
            .Bind(x=> ScriptGenerator.CompileScript(x).MapError(e=>e.WithLocation(this)));


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
        protected override async Task<Result<Unit, IError>> RunAsync(StateMonad stateMonad, CancellationToken cancellationToken)
        {
            var settingsResult = stateMonad.GetSettings<INuixSettings>().MapError(x=>x.WithLocation(this));
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
                var blockResult = await TryGetRubyBlock(stateMonad, cancellationToken);//This will run child functions
                if (blockResult.IsFailure)
                    return blockResult.ConvertFailure<Unit>();
                block = blockResult.Value;

            }

            var r = await RubyBlockRunner.RunAsync(FunctionName, block, stateMonad, settingsResult.Value, cancellationToken)
                .MapError(x=>x.WithLocation(this));

            return r;
        }
    }

}