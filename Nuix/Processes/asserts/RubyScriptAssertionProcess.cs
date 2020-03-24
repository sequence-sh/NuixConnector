using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.immutable;

namespace Reductech.EDR.Connectors.Nuix.processes.asserts
{
    /// <summary>
    /// A ruby script that will fail if a particular thing does not happen.
    /// </summary>
    public abstract class RubyScriptAssertionProcess : RubyScriptProcess
    {
        /// <inheritdoc />
        internal override Result<ImmutableProcess, ErrorList> TryGetImmutableProcess(string name, string nuixExeConsolePath, bool useDongle, IReadOnlyCollection<string> methodSet,
            IReadOnlyCollection<ImmutableRubyScriptProcess.MethodCall> methodCalls)
        {
            return Result.Success<ImmutableProcess, ErrorList>(
                new ImmutableRubyScriptAssertionProcess(name, nuixExeConsolePath, useDongle, methodSet, methodCalls, InterpretLine, DefaultFailureMessage));
        }


        /// <summary>
        /// Interprets the line as a success or failure. If its a failure, also returns a message.
        /// </summary>
        /// <returns></returns>
        protected abstract (bool success, string? failureMessage)? InterpretLine(string s);

        

        /// <summary>
        /// The message to send if the assertion fails
        /// </summary>
        protected abstract string DefaultFailureMessage { get; }

        
    }
}