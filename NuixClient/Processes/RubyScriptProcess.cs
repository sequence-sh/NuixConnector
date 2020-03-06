using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using Processes;
using Processes.process;

namespace NuixClient.processes
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
        /// Execute this process
        /// </summary>
        /// <returns></returns>
        public sealed override async IAsyncEnumerable<Result<string>> Execute()
        {
            var argumentErrors = GetArgumentErrors().ToList();

            if (argumentErrors.Any())
            {
                foreach (var ae in argumentErrors)
                    yield return Result.Failure<string>(ae);
                yield break;
            }

            var currentDirectory = Directory.GetCurrentDirectory();
            var scriptPath = Path.Combine(currentDirectory, "..", "nuixclient", "NuixClient", "scripts", ScriptName);
            
            var args = new List<string>();

            foreach (var (key, value) in GetArgumentValuePairs())
            {
                args.Add(key);
                args.Add(value.Replace(@"\", @"\\"));//Escape backslashes
            }

            var processState = new ProcessState();

            var arguments = new List<string>();

            var useDongleString =  ConfigurationManager.AppSettings["NuixUseDongle"];

            if(!string.IsNullOrWhiteSpace(useDongleString))
                if (bool.TryParse(useDongleString, out var useDongle))
                {
                    if (useDongle)
                    {
                        // ReSharper disable once StringLiteralTypo
                        arguments.Add("-licencesourcetype");
                        arguments.Add("dongle");  
                    }
                }
                else
                {
                    yield return Result.Failure<string>($"Setting 'NuixUseDongle' must be either 'True' or 'False'");
                    yield break;
                }

                
            if (!string.IsNullOrWhiteSpace(scriptPath))
                arguments.Add(scriptPath);

            arguments.AddRange(args);

            var nuixExeConsolePath = ConfigurationManager.AppSettings["NuixExeConsolePath"];

            if (string.IsNullOrWhiteSpace(nuixExeConsolePath))
            {
                yield return Result.Failure<string>($"Setting 'NuixExeConsolePath' must be set");
                yield break;
            }

            await foreach (var rl in ExternalProcessHelper.RunExternalProcess(nuixExeConsolePath, arguments))
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