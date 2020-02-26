using System;
using System.Collections.Generic;
using System.IO;
using CSharpFunctionalExtensions;

namespace Orchestration
{
    /// <summary>
    /// Runs external processes.
    /// </summary>
    public static class ExternalProcessHelper
    {
        /// <summary>
        /// Runs an external process and returns the output and errors
        /// </summary>
        /// <param name="processPath">The path to the process</param>
        /// <param name="arguments">The arguments to provide to the process</param>
        /// <returns>The output of the process</returns>
        public static async IAsyncEnumerable<Result<string>> RunExternalProcess(string processPath, IEnumerable<string> arguments)
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
                yield return Result.Success(line);
            }

            pProcess.WaitForExit();
        }
    }
}