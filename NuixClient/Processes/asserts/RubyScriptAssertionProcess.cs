using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Processes.process;

namespace NuixClient.processes.asserts
{
    /// <summary>
    /// A ruby script that will fail if a particular thing does not happen.
    /// </summary>
    public abstract class RubyScriptAssertionProcess : RubyScriptProcess
    {
        internal override bool HandleLine(Result<string> rl, ProcessState processState)
        {
            if (rl.IsFailure) //Some sort of error => the assertion should fail and we want to see the error message
                processState.Artifacts[SuccessArtifactName] = false;
            else// if (!processState.Artifacts.ContainsKey(SuccessArtifactName))
            {
                var i = InterpretLine(rl.Value);
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

        /// <summary>
        /// Interprets the line as a success or failure. If its a failure, also returns a message.
        /// </summary>
        /// <returns></returns>
        protected abstract (bool success, string? failureMessage)? InterpretLine(string s);

        

        /// <summary>
        /// The message to send if the assertion fails
        /// </summary>
        protected abstract string DefaultFailureMessage { get; }

        private const string SuccessArtifactName = "Success";

        private const string FailureMessageArtifactName = "FailureMessage";

        internal override IEnumerable<Result<string>> OnScriptFinish(ProcessState processState)
        {
            if (processState.Artifacts.TryGetValue(SuccessArtifactName, out var sObject) && sObject is bool b && b) yield break;

            var failureMessage = processState.Artifacts.TryGetValue(FailureMessageArtifactName, out var fm) && fm != null? fm.ToString() : DefaultFailureMessage;
            
            yield return Result.Failure<string>(failureMessage);
        }
    }
}