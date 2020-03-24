using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.immutable;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    internal class ImmutableRubyScriptProcess : ImmutableProcess
    {
        /// <inheritdoc />
        public ImmutableRubyScriptProcess(string name, string nuixExeConsolePath, IReadOnlyCollection<string> arguments) : base(name)
        {
            _nuixExeConsolePath = nuixExeConsolePath;
            _arguments = arguments;
        }

        private readonly string _nuixExeConsolePath;

        private readonly IReadOnlyCollection<string> _arguments;

        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute()
        {
            var processState = new ProcessState();

            foreach (var lb in BeforeScriptStart(processState))
                yield return lb;

            await foreach (var rl in ExternalProcessHelper.RunExternalProcess(_nuixExeConsolePath, _arguments))
            {
                if(HandleLine(rl, processState))
                    yield return rl;
            }

            foreach (var l in OnScriptFinish(processState))
                yield return l;
        }
        /// <summary>
        /// Do something with a line returned from the script
        /// </summary>
        /// <param name="rl">The line to look at</param>
        /// <param name="processState">The current state of the process</param>
        /// <returns>True if the line should continue through the pipeline</returns>
        internal virtual bool HandleLine(Result<string> rl, ProcessState processState)
        {
            return true;
        }

        /// <summary>
        /// What to do before the script starts.
        /// </summary>
        internal virtual IEnumerable<Result<string>> BeforeScriptStart(ProcessState processState)
        {
            yield break;
        }

        /// <summary>
        /// What to do when the script finishes
        /// </summary>
        internal virtual IEnumerable<Result<string>> OnScriptFinish(ProcessState processState)
        {
            yield break;
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (!(obj is ImmutableRubyScriptProcess rsp))
                return false;

            return Name == rsp.Name && _nuixExeConsolePath.Equals(rsp._nuixExeConsolePath) &&
                   _arguments.SequenceEqual(rsp._arguments);
        }
    }
}