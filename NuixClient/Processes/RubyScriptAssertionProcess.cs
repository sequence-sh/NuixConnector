using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Processes.process;

namespace NuixClient.processes
{
    /// <summary>
    /// A ruby script that will fail if a particular thing does not happen.
    /// </summary>
    public abstract class RubyScriptAssertionProcess : RubyScriptProcess
    {
        internal override bool HandleLine(Result<string> rl, ProcessState processState)
        {
            if (rl.IsFailure) //Some sort of error => the assertion should fail and we want to see the error message
                _isSuccessful = false;
            else if (_isSuccessful == null)
            {
                var i = InterpretLine(rl.Value);
                if(i != null) //we've understood this line
                {
                    if(_isSuccessful != false) //If the assertion has failed, nothing can change it
                        _isSuccessful = i switch
                        {
                            false => false,
                            true => true,
                            _ => _isSuccessful
                        };
                    return false;
                }                
            }

            return base.HandleLine(rl, processState);
        }

        /// <summary>
        /// If true this assertion is successful.
        /// If false this assertion is a failure.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        protected abstract bool? InterpretLine(string s);

        private bool? _isSuccessful;

        /// <summary>
        /// The message to send if the assertion fails
        /// </summary>
        protected abstract string FailureMessage { get; }

        internal override IEnumerable<Result<string>> OnScriptFinish(ProcessState processState)
        {
            if (_isSuccessful == null || !_isSuccessful.Value)
            {
                yield return Result.Failure<string>(FailureMessage);
            }
        }
    }
}