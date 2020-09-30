using System;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    internal static class RubyBlockRunner
    {
        public static async Task<Result<Unit, IRunErrors>> RunAsync(string name, IUnitRubyBlock block, StateMonad stateMonad, INuixSettings settings)
        {
            var argumentsResult = ScriptGenerator.CompileScript(block)
                .Bind(st => RubyScriptCompilationHelper.TryGetTrueArgumentsAsync(st, settings, block)).Result;

            if (argumentsResult.IsFailure)
                return argumentsResult.ConvertFailure<Unit>();


            var logger = new ScriptStepLogger(stateMonad);

            var result = await stateMonad.ExternalProcessRunner.RunExternalProcess(settings.NuixExeConsolePath, logger, name, NuixErrorHandler.Instance, argumentsResult.Value);

            if (result.IsFailure)
                return result;

            if (logger.Completed)
                return Unit.Default;

            return new RunError("Nuix function did not complete successfully", name, null, ErrorCode.ExternalProcessMissingOutput);
        }


        internal sealed class ScriptStepLogger : ILogger
        {
            public ScriptStepLogger(StateMonad stateMonad) => StateMonad = stateMonad;

            public StateMonad StateMonad { get; }

            public bool Completed { get; private set; } = false;

            /// <inheritdoc />
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (state?.ToString() == ScriptGenerator.UnitSuccessToken)
                    Completed = true;
                else
                    StateMonad.Logger.Log(logLevel, eventId, state, exception, formatter);
            }

            /// <inheritdoc />
            public bool IsEnabled(LogLevel logLevel) => StateMonad.Logger.IsEnabled(logLevel);

            /// <inheritdoc />
            public IDisposable BeginScope<TState>(TState state) => StateMonad.Logger.BeginScope(state);

        }
    }
}