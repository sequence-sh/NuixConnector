using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using Orchestration;
using Orchestration.Processes;


namespace NuixClient.Processes
{
    /// <summary>
    /// A process that runs a ruby script against NUIX
    /// </summary>
    public abstract class RubyScriptProcess : Process
    {
        //TODO make a config property
        private const string NuixExeConsolePath = @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe";
        //TODO make a config property
        private const bool UseDongle = true;
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
        /// What to do when the script finishes
        /// </summary>
        internal virtual void OnScriptFinish(ProcessState processState)
        {
        }
    
        /// <summary>
        /// Execute this process
        /// </summary>
        /// <returns></returns>
        public override async IAsyncEnumerable<Result<string>> Execute()
        {
            var argumentErrors = GetArgumentErrors().ToList();

            if (argumentErrors.Any())
            {
                foreach (var ae in argumentErrors)
                    yield return Result.Failure<string>(ae);
                yield break;
            }

            var currentDirectory = Directory.GetCurrentDirectory();
            var scriptPath = Path.Combine(currentDirectory, "..", "nuixclient", "NuixClient", "Scripts", ScriptName);
            
            var args = new List<string>();

            foreach (var (key, value) in GetArgumentValuePairs())
            {
                args.Add(key);
                args.Add(value);
            }

            var processState = new ProcessState();

            var arguments = new List<string>();

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (UseDongle)
                // ReSharper disable once StringLiteralTypo
                arguments.Add("-licencesourcetype dongle");
            if (!string.IsNullOrWhiteSpace(scriptPath))
                arguments.Add(scriptPath);

            arguments.AddRange(args);

            await foreach (var rl in ExternalProcessHelper.RunExternalProcess(NuixExeConsolePath, arguments))
            {
                if(HandleLine(rl, processState))
                    yield return rl;
            }

            OnScriptFinish(processState);
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