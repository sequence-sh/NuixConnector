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

            if (useDongle)
                arguments.Add("-licencesourcetype dongle");
            if (!string.IsNullOrWhiteSpace(scriptPath))
                arguments.Add(scriptPath);

            arguments.AddRange(scriptArguments);

            if (!File.Exists(nuixConsoleExePath))
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

            //Read the output one line at a time //TODO find a way to get the full output and the full error
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

        #region obsolete

        //This doesn't work because I can't figure out how to pass arguments
        private static async IAsyncEnumerable<string> CreateCaseECMA(
            string nuixConsoleExePath = @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe",
            string casePath = @"C:\Dev\Nuix\Cases\MyNewCase2",
            string caseName = "MyNewCase2",
            string description = "Description",
            string investigator = "Investigator",
            bool useDongle = true)
        {

            var scriptPath = @"C:\Source\Repos\NuixClient\Scripts\CreateCase.js";

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


        //this doesn't work because I can't find out how to pass the arguments
        private static async IAsyncEnumerable<string> CreateCasePython(
            string casePath, // = @"C:\Dev\Nuix\Cases\MyNewCase2",
            string caseName, // = "MyNewCase2",
            string description, // = "Description",
            string investigator, // = "Investigator",
            string nuixConsoleExePath = @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe",
            bool useDongle = true)
        {
            //var currentDirectory = Directory.GetCurrentDirectory();
            //var currentDirectory = @"C:\Source\Repos\NuixClient";
            //var scriptPath = Path.Combine(currentDirectory, "Scripts", "CreateCase.py");

            var scriptPath = @"C:\Source\Repos\NuixClient\Scripts\CreateCase.py";

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

        #endregion obsolete



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

            string casePath, //= @"C:\Dev\Nuix\Cases\MyNewCase",
            string caseName, // = "MyNewCase", 
            string description, //= "Description",
            string investigator, // = "Investigator",
            string nuixConsoleExePath = @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe",
            bool useDongle = true)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var scriptPath = Path.Combine(currentDirectory, "..", "NuixClient", "Scripts", "CreateCase.rb");

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


        //"yyyy-MM-dd'T'HH:mm:ss.SSSZ" 

        /// <summary>
        /// Add file to a Case in NUIX
        /// </summary>
        /// <param name="nuixConsoleExePath">Path to the console exe</param>
        /// <param name="casePath">Path of the case to open</param>
        /// <param name="folderName">The name of the folder to create</param>
        /// <param name="description">Description of the new folder</param>
        /// <param name="custodian">Custodian for the new folder</param>
        /// <param name="filePath">The path of the file to add</param>
        /// <param name="useDongle">Use a dongle for licensing</param>
        /// <returns>The output of the case creation script</returns>
        public static async IAsyncEnumerable<string> AddFileToCase( //TODO remove default arguments

            string casePath = @"C:\Dev\Nuix\Cases\NewCase",
            string folderName = "TestFolder",
            string description = "nice",
            string custodian = "mark2",
            string filePath = @"C:\Dev\Nuix\Data\Custodians\BobS\Report3.ufdr",
            string nuixConsoleExePath = @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe",
            bool useDongle = true)
        {
            //var currentDirectory = Directory.GetCurrentDirectory();
            var currentDirectory = @"C:\Source\Repos\NuixClient";
            var scriptPath = Path.Combine(currentDirectory, "Scripts", "AddToCase.rb");

            var args = new[]
            {
                "-p", casePath,
                "-n", folderName,
                "-d", description,
                "-c", custodian,
                "-f", filePath
            };
            var result = RunScript(nuixConsoleExePath, scriptPath, useDongle, args);

            await foreach (var line in result)
                yield return line;
        }


        /// <summary>
        /// Add concordance to a case in NUIX
        /// </summary>
        /// <param name="nuixConsoleExePath">Path to the console exe</param>
        /// <param name="concordanceProfileName">Name of the concordance profile to use</param>
        /// <param name="casePath">Path of the case to open</param>
        /// <param name="folderName">The name of the folder to create</param>
        /// <param name="description">Description of the new folder</param>
        /// <param name="custodian">Custodian for the new folder</param>
        /// <param name="filePath">The path of the file to add</param>
        /// <param name="concordanceDateFormat">Concordance date format to use</param>
        /// <param name="useDongle">Use a dongle for licensing</param>
        /// <returns>The output of the case creation script</returns>
        public static async IAsyncEnumerable<string> AddConcordanceToCase( //TODO remove default arguments

            string casePath = @"C:\Dev\Nuix\Cases\NewCase",
            string folderName = "BestFolder",
            string description = "nice",
            string custodian = "mw",
            string filePath = @"C:\Dev\Nuix\Exports\Export1\loadfile.dat",
            string concordanceDateFormat = @"yyyy-MM-dd'T'HH:mm:ss.SSSZ",
            string nuixConsoleExePath = @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe",
            string concordanceProfileName = @"TestProfile",
            bool useDongle = true)
        {
            //var currentDirectory = Directory.GetCurrentDirectory();
            var currentDirectory = @"C:\Source\Repos\NuixClient";
            var scriptPath = Path.Combine(currentDirectory, "Scripts", "AddConcordanceToCase.rb");

            var args = new[]
            {
                "-p", casePath,
                "-n", folderName,
                "-d", description,
                "-c", custodian,
                "-f", filePath,
                "-z", concordanceDateFormat,
                "-t", concordanceProfileName
            };
            var result = RunScript(nuixConsoleExePath, scriptPath, useDongle, args);

            await foreach (var line in result)
                yield return line;
        }

        /// <summary>
        /// Export concordance from a production set in NUIX
        /// </summary>
        /// <param name="nuixConsoleExePath">Path to the nuix console exe</param>
        /// <param name="casePath">Path of the case to open</param>
        /// <param name="exportPath">The path to export to</param>
        /// <param name="productionSetName">The name of the production set to export</param>
        /// <param name="metadataProfileName">Optional name of the metadata profile to use. Case sensitive. Note this is NOT a metadata export profile</param>
        /// <param name="useDongle">Use a dongle for licensing</param>
        /// ///
        /// <returns>The output of the case creation script</returns>
        public static async IAsyncEnumerable<string> ExportProductionSetConcordance( //TODO remove default arguments
            string nuixConsoleExePath = @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe",
            string casePath = @"C:\Dev\Nuix\Cases\NewCase",
            string exportPath = @"C:\Dev\Nuix\Exports\Export6",
            string productionSetName = @"Night",
            string metadataProfileName = "Default",
            bool useDongle = true)
        {
            //var currentDirectory = Directory.GetCurrentDirectory();
            var currentDirectory = @"C:\Source\Repos\NuixClient";
            var scriptPath = Path.Combine(currentDirectory, "Scripts", "ExportConcordance.rb");

            var args = new[]
            {
                "-p", casePath,
                "-x", exportPath,
                "-n", productionSetName,
                "-m", metadataProfileName
            };
            var result = RunScript(nuixConsoleExePath, scriptPath, useDongle, args);

            await foreach (var line in result)
                yield return line;
        }


    }


    


}

