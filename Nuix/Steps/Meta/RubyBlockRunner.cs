using System;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    internal static class RubyBlockRunner
    {
        public static async Task<Result<Unit, IErrorBuilder>> RunAsync(string name, IUnitRubyBlock block, StateMonad stateMonad, INuixSettings settings, CancellationToken cancellationToken)
        {
            var arguments = await RubyScriptCompilationHelper.PrepareScriptAsync(block, stateMonad, settings, cancellationToken);

            if (arguments.IsFailure) return arguments.ConvertFailure<Unit>();

            var logger = new ScriptStepLogger(stateMonad);

            var result = await stateMonad.ExternalProcessRunner
                .RunExternalProcess(settings.NuixExeConsolePath, logger, NuixErrorHandler.Instance, arguments.Value);

            if (result.IsFailure)
                return result;

            if (logger.Completed)
                return Unit.Default;

            return new ErrorBuilder("Nuix function did not complete successfully", ErrorCode.ExternalProcessMissingOutput);
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