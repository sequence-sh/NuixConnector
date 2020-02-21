using System;
using System.Collections.Generic;
using System.IO;

namespace NuixClient
{
    /// <summary>
    /// Runs nuix scripts externally
    /// </summary>
    internal static class ScriptRunner
    {
        /// <summary>
        /// Runs an external process and returns the output and errors
        /// </summary>
        /// <param name="processPath">The path to the process</param>
        /// <param name="arguments">The arguments to provide to the process</param>
        /// <returns>The output of the process</returns>
        internal static async IAsyncEnumerable<ResultLine> RunExternalProcess(string processPath, IEnumerable<string> arguments)
        {
            if (!File.Exists(processPath))
                throw new Exception($"Could not find '{processPath}'");

            using var pProcess = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = processPath,
                    Arguments = string.Join(' ', arguments),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden, //don't display a window
                    CreateNoWindow = true
                }
            };
            pProcess.Start();

            var multiStreamReader = new MultiStreamReader(new[]
            {
                new WrappedStreamReader(pProcess.StandardOutput),
                new WrappedStreamReader(pProcess.StandardError),
            });

            //Read the output one line at a time
            while (true)
            {
                var line = await multiStreamReader.ReadLineAsync();
                if (line == null) //We've reached the end of the file
                    break;
                yield return new ResultLine(true, line);
            }

            pProcess.WaitForExit();
        }

        /// <summary>
        /// Runs a nuix script on a local instance of nuix console.
        /// Returns the output one line at a time
        /// </summary>
        /// <param name="nuixConsoleExePath">The path to the nuix_console executable</param>
        /// <param name="scriptPath">The path to the script</param>
        /// <param name="useDongle"></param>
        /// <param name="scriptArguments">Arguments to the script</param>
        internal static async IAsyncEnumerable<ResultLine> RunScript(string nuixConsoleExePath,
            string scriptPath,
            bool useDongle,
            IEnumerable<string> scriptArguments)
        {
            var arguments = new List<string>();

            if (useDongle)
                // ReSharper disable once StringLiteralTypo
                arguments.Add("-licencesourcetype dongle");
            if (!string.IsNullOrWhiteSpace(scriptPath))
                arguments.Add(scriptPath);

            arguments.AddRange(scriptArguments);

            await foreach (var rl in RunExternalProcess(nuixConsoleExePath, arguments))
                yield return rl;
        }
    }
}