using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
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
            .Bind(b=>ScriptGenerator.CompileScript(RubyScriptProcessFactory.RubyFunction.FunctionName, b));


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
                if (blockResult.IsFailure) return blockResult.ConvertFailure<Unit>();
                block = blockResult.Value;

            }

            var argumentsResult = ScriptGenerator.CompileScript(Name, block)
                    .Bind(st=> RubyScriptCompilationHelper.TryGetTrueArgumentsAsync(st, settingsResult.Value, block)).Result;

            if (argumentsResult.IsFailure)
                return argumentsResult.ConvertFailure<Unit>();


            var logger = new ScriptProcessLogger(processState);

            var result = await ExternalProcessMethods.RunExternalProcess(settingsResult.Value.NuixExeConsolePath, logger, Name, argumentsResult.Value);

            if (result.IsFailure)
                return result;

            if (logger.Completed)
                return Unit.Default;

            return new RunError("Nuix function did not complete successfully", Name, null, ErrorCode.ExternalProcessMissingOutput);
        }

        internal sealed class ScriptProcessLogger : ILogger
        {
            public ScriptProcessLogger(ProcessState processState) => ProcessState = processState;

            public ProcessState ProcessState { get; }

            public bool Completed { get; private set; } = false;

            /// <inheritdoc />
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (state?.ToString() == ScriptGenerator.UnitSuccessToken)
                    Completed = true;
                else
                    ProcessState.Logger.Log(logLevel, eventId, state, exception, formatter);
            }

            /// <inheritdoc />
            public bool IsEnabled(LogLevel logLevel) => ProcessState.Logger.IsEnabled(logLevel);

            /// <inheritdoc />
            public IDisposable BeginScope<TState>(TState state) => ProcessState.Logger.BeginScope(state);

        }

    }

}