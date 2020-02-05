using System;
using System.Collections.Generic;
using System.IO;

namespace NuixClient
{
    /// <summary>
    /// Methods for running nuix scripts via the console
    /// </summary>
    public static class OutsideScripting
    {
        /// <summary>
        /// Runs a nuix script on a local instance of nuix console.
        /// Returns the output one line at a time
        /// </summary>
        /// <param name="nuixConsoleExePath">The path to the nuix_console executable</param>
        /// <param name="scriptPath">The path to the script</param>
        /// <param name="useDongle"></param>
        /// <param name="scriptArguments">Arguments to the script</param>
        public static async IAsyncEnumerable<string> RunScript(string nuixConsoleExePath,
            string scriptPath,
            bool useDongle,
            IEnumerable<string> scriptArguments)
        {
            var arguments = new List<string>();

            if(useDongle)
                arguments.Add("-licencesourcetype dongle");
            if(!string.IsNullOrWhiteSpace(scriptPath))
                arguments.Add(scriptPath);

            arguments.AddRange(scriptArguments);

            if(!File.Exists(nuixConsoleExePath))
                throw new Exception($"Could not find '{nuixConsoleExePath}'");

            using var pProcess = new System.Diagnostics.Process
            {
                StartInfo =
                {
                    FileName = nuixConsoleExePath,
                    Arguments = string.Join(' ', arguments),
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden, //don't display a window
                    CreateNoWindow = true
                }
            };
            pProcess.Start();

            //Read the output one line at a time
            while (true)
            {
                var line = await pProcess.StandardOutput.ReadLineAsync();
                if (line == null) //We've reached the end of the file
                    break;
                yield return line;
            }

            //Read the error one line at a time
            while (true)
            {
                var line = await pProcess.StandardError.ReadLineAsync();
                if (line == null) //We've reached the end of the file
                    break;
                yield return line;
            }

            pProcess.WaitForExit();
        }

        public static async IAsyncEnumerable<string> CreateCasePython(
            string nuixConsoleExePath = @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe",
            string casePath = @"C:\Dev\Nuix\Cases\MyNewCase2",
            string caseName = "MyNewCase2",
            string description = "Description",
            string investigator = "Investigator",
            bool useDongle = true)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var scriptPath = Path.Combine(currentDirectory, "Scripts", "CreateCase.py");

            var args = new[]
            {
                "-p", casePath,
                "-n", caseName,
                "-d", description,
                "-i", investigator
            };
            var result = RunScript(nuixConsoleExePath, scriptPath, useDongle, args);

            await foreach (var line in result)
                yield return line;
        }
    /// <summary>
    /// Creates a new Case in NUIX
    /// </summary>
    /// <param name="nuixConsoleExePath">Path to the console exe</param>
    /// <param name="casePath">Where to create the new case</param>
    /// <param name="caseName">The name of the new case</param>
    /// <param name="description">Description of the case</param>
    /// <param name="investigator">Name of the investigator</param>
    /// <param name="useDongle">Use a dongle for licensing</param>
    /// <returns>The output of the case creation script</returns>
    public static async IAsyncEnumerable<string> CreateCaseRuby( //TODO remove default arguments
            string nuixConsoleExePath = @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe", 
            string casePath= @"C:\Dev\Nuix\Cases\MyNewCase",
            string caseName = "MyNewCase", 
            string description= "Description",
            string investigator = "Investigator",
            
            
            bool useDongle = true)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var scriptPath = Path.Combine(currentDirectory, "Scripts", "CreateCase.rb");

            var args = new[]
                {
                "-p", casePath,
                "-n", caseName,
                "-d", description,
                "-i", investigator
                };
            var result = RunScript(nuixConsoleExePath, scriptPath, useDongle, args);

            await foreach(var line in result)
                yield return line;
        }


    }
}
