using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    internal class ImmutableRubyScriptAssertionProcess : ImmutableRubyScriptProcess
    {
        /// <inheritdoc />
        public ImmutableRubyScriptAssertionProcess(
            string name, 
            string nuixExeConsolePath, 
            IReadOnlyCollection<string> arguments, 
            Func<string, (bool success, string? failureMessage)?> interpretLine, string defaultFailureMessage) : base(name, nuixExeConsolePath, arguments)
        {
            _interpretLine = interpretLine;
            _defaultFailureMessage = defaultFailureMessage;
        }

        private readonly Func<string, (bool success, string? failureMessage)?> _interpretLine;
        /// <summary>
        /// The message to send if the assertion fails
        /// </summary>
        private readonly string _defaultFailureMessage;


        internal override bool HandleLine(Result<string> rl, ProcessState processState)
        {
            if (rl.IsFailure) //Some sort of error => the assertion should fail and we want to see the error message
                processState.Artifacts[SuccessArtifactName] = false;
            else// if (!processState.Artifacts.ContainsKey(SuccessArtifactName))
            {
                var i = _interpretLine(rl.Value);
                if(i != null) //we've understood this line
                {
                    if (!processState.Artifacts.TryGetValue(SuccessArtifactName, out var currentState) ||
                        !currentState.Equals(false))
                    {//Only change the current state if we haven't already failed
                        processState.Artifacts[SuccessArtifactName] = i.Value.success;
                    }

                    if (i.Value.failureMessage != null &&
                        !processState.Artifacts.ContainsKey(FailureMessageArtifactName))
                        processState.Artifacts[FailureMessageArtifactName] = i.Value.failureMessage;
                    return false;
                }                
            }

            return base.HandleLine(rl, processState);
        }

        private const string SuccessArtifactName = "Success";

        private const string FailureMessageArtifactName = "FailureMessage";

        internal override IEnumerable<Result<string>> OnScriptFinish(ProcessState processState)
        {
            if (processState.Artifacts.TryGetValue(SuccessArtifactName, out var sObject) && sObject is bool b && b) yield break;

            var failureMessage = processState.Artifacts.TryGetValue(FailureMessageArtifactName, out var fm) && fm != null? fm.ToString() : _defaultFailureMessage;
            
            yield return Result.Failure<string>(failureMessage);
        }

    }
}