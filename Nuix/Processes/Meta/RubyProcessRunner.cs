using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Connectors.Nuix.Processes.Meta
{
    internal static class RubyProcessRunner
    {
        public static async Task<Result<Unit, IRunErrors>> RunAsync(string name, IUnitRubyBlock block, ProcessState processState, INuixProcessSettings settings)
        {
            var argumentsResult = ScriptGenerator.CompileScript(block)
                .Bind(st => RubyScriptCompilationHelper.TryGetTrueArgumentsAsync(st, settings, block)).Result;

            if (argumentsResult.IsFailure)
                return argumentsResult.ConvertFailure<Unit>();


            var logger = new ScriptProcessLogger(processState);

            var result = await processState.ExternalProcessRunner.RunExternalProcess(settings.NuixExeConsolePath, logger, name, NuixErrorHandler.Instance, argumentsResult.Value);

            if (result.IsFailure)
                return result;

            if (logger.Completed)
                return Unit.Default;

            return new RunError("Nuix function did not complete successfully", name, null, ErrorCode.ExternalProcessMissingOutput);
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