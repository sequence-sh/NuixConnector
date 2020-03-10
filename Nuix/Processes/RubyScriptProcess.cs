using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// A process that runs a ruby script against NUIX
    /// </summary>
    public abstract class RubyScriptProcess : Process
    {
        /// <summary>
        /// Checks if the current set of arguments is valid
        /// </summary>
        /// <returns></returns>
        internal abstract string ScriptName { get; }

        internal abstract IEnumerable<(string arg, string val)> GetArgumentValuePairs();

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

        /// <summary>
        /// Gets errors in the settings object
        /// </summary>
        /// <param name="processSettings"></param>
        /// <returns></returns>
        public override IEnumerable<string> GetSettingsErrors(IProcessSettings processSettings)
        {
            if (!(processSettings is INuixProcessSettings nps))
            {
                yield return $"Process settings must be an instance of {nameof(INuixProcessSettings)}";
                yield break;
            }

            if (string.IsNullOrWhiteSpace(nps.NuixExeConsolePath))
                yield return $"'{nameof(nps.NuixExeConsolePath)}' must not be empty";
        }

        /// <summary>
        /// Execute this process
        /// </summary>
        /// <returns></returns>
        public sealed override async IAsyncEnumerable<Result<string>> Execute(IProcessSettings processSettings)
        {
            if (!(processSettings is INuixProcessSettings nuixProcessSettings))
            {
                yield return Result.Failure<string>($"Process Settings must be an instance of {typeof(INuixProcessSettings).Name}");
                yield break;
            }

            var errors = GetArgumentErrors().Concat(GetSettingsErrors(processSettings)).ToList();

            if (errors.Any())
            {
                foreach (var ae in errors)
                    yield return Result.Failure<string>(ae);
                yield break;
            }


            var currentDirectory = System.AppContext.BaseDirectory;
            var scriptPath = Path.Combine(currentDirectory,  "scripts", ScriptName);
            
            var args = new List<string>();

            foreach (var (key, value) in GetArgumentValuePairs())
            {
                args.Add(key);
                args.Add(value.Replace(@"\", @"\\"));//Escape backslashes
            }

            var processState = new ProcessState();

            var arguments = new List<string>();

            if (nuixProcessSettings.UseDongle)
            {
                // ReSharper disable once StringLiteralTypo
                arguments.Add("-licencesourcetype");
                arguments.Add("dongle");  
            }

            if (!string.IsNullOrWhiteSpace(scriptPath))
                arguments.Add(scriptPath);

            arguments.AddRange(args);


            await foreach (var rl in ExternalProcessHelper.RunExternalProcess(nuixProcessSettings.NuixExeConsolePath, arguments))
            {
                if(HandleLine(rl, processState))
                    yield return rl;
            }

            foreach (var l in OnScriptFinish(processState))
                yield return l;
        }

        /// <summary>
        /// Determines if two processes are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return obj is RubyScriptProcess rsp && ScriptName == rsp.ScriptName &&
                   GetArgumentValuePairs().SequenceEqual(rsp.GetArgumentValuePairs());
        }

        /// <summary>
        /// Get the hash code for this process.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            var t = 2;

            unchecked
            {
                t += 3 * GetType().GetHashCode();

                // ReSharper disable once LoopCanBeConvertedToQuery - possible overflow exception
                foreach (var argumentValuePair in GetArgumentValuePairs()) 
                    t += argumentValuePair.GetHashCode();
            }
            

            return t;
        }
    }
}