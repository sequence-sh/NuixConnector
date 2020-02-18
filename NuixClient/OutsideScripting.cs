using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using NuixClient.enums;

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
        private static async IAsyncEnumerable<ResultLine> RunScript(string nuixConsoleExePath,
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

            var multiStreamReader = new MultiStreamReader(new []
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
        /// Creates a new Case in NUIX
        /// </summary>
        /// <param name="nuixConsoleExePath">Path to the console exe</param>
        /// <param name="casePath">Where to create the new case</param>
        /// <param name="useDongle">Use a dongle for licensing</param>
        /// <param name="searchTerm">The term to search for</param>
        /// <param name="tag">The tag to apply to the found terms</param>
        /// <param name="order">Order by term e.g. name ASC</param>
        /// <param name="limit">Optional maximum number of items to tag.</param>
        /// <returns>The output of the case creation script</returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> SearchAndTag( 

            string casePath= @"C:\Dev\Nuix\Cases\NewCase",
            string searchTerm = "night",
            string tag  = "Nocturnal",
            string? order = null,
            int? limit = null,
            string nuixConsoleExePath = @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe",
            bool useDongle = true)
        {
            var (searchTermParseSuccess, searchTermParseError, searchTermParsed) = Search.SearchParser.TryParse(searchTerm);

            if (!searchTermParseSuccess || searchTermParsed == null)
            {
                yield return new ResultLine(false, "Error parsing search term");
                if(searchTermParseError != null)
                    yield return new ResultLine(false, searchTermParseError);
                yield break;
            }

            if (searchTermParsed.AsString != searchTerm)
            {
                yield return new ResultLine(true, $"Search term simplified to '{searchTermParsed.AsString}'");
            }

            var currentDirectory = Directory.GetCurrentDirectory();
            var scriptPath = Path.Combine(currentDirectory, "..", "NuixClient", "Scripts", "SearchAndTag.rb");

            var args = new List<string>
            {
                "-p", casePath,
                "-s", searchTermParsed.AsString,
                "-t", tag,
                
            };
            if (order != null)
            {
                args.Add("-o ");
                args.Add(order);
            }

            if (limit.HasValue)
            {
                args.Add("-l");
                args.Add(limit.Value.ToString());
            }

            var result = RunScript(nuixConsoleExePath, scriptPath, useDongle, args);

            await foreach (var line in result)
                yield return line;
        }

        /// <summary>
        /// Add items to an item set in NUIX
        /// </summary>
        /// <param name="casePath">Where to create the new case</param>
        /// <param name="searchTerm">The term to search for</param>
        /// <param name="itemSetName">The item set to add the found items to. Will be created if it doesn't exist</param>
        /// <param name="order">Order by term e.g. name ASC</param>
        /// <param name="limit">Optional maximum number of items to tag.</param>
        /// <param name="itemSetDeduplication">The means of deduplicating items by key and prioritizing originals in a tie-break. </param>
        /// <param name="itemSetDescription">The description of the item set as a string.</param>
        /// <param name="deduplicateBy">Whether to deduplicate as a family or individual</param>
        /// <param name="custodianRanking">A list of custodian names ordered from highest ranked to lowest ranked. If this parameter is present and the deduplication parameter has not been specified, MD5 Ranked Custodian is assumed.</param>
        /// <param name="nuixConsoleExePath">Path to the console exe</param>
        /// <param name="useDongle">Use a dongle for licensing</param>

        /// <returns>The output of the case creation script</returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> AddToItemSet(
            string casePath = @"C:\Dev\Nuix\Cases\NewCase",
            string searchTerm = "night",
            string itemSetName = "ProdSet",
            string? order = null,
            int? limit = null,
            ItemSetDeduplication itemSetDeduplication = ItemSetDeduplication.Default,
            string? itemSetDescription = null,
            DeduplicateBy deduplicateBy = DeduplicateBy.Individual,
            string[]? custodianRanking = null,
            string nuixConsoleExePath = @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe",
            bool useDongle = true
        )
        {
            //TODO check other arguments are valid

            var (searchTermParseSuccess, searchTermParseError, searchTermParsed) = Search.SearchParser.TryParse(searchTerm);

            if (!searchTermParseSuccess || searchTermParsed == null)
            {
                yield return new ResultLine(false, "Error parsing search term");
                if (searchTermParseError != null)
                    yield return new ResultLine(false, searchTermParseError);
                yield break;
            }

            if (searchTermParsed.AsString != searchTerm)
            {
                yield return new ResultLine(true, $"Search term simplified to '{searchTermParsed.AsString}'");
            }

            var currentDirectory = Directory.GetCurrentDirectory();
            var scriptPath = Path.Combine(currentDirectory, "..", "nuixclient", "NuixClient", "Scripts", "AddToItemSet.rb");

            var args = new List<string>
            {
                "-p", casePath,
                "-s", searchTermParsed.AsString,
                "-n", itemSetName,

            };
            if (order != null)
            {
                args.Add("-o ");
                args.Add(order);
            }

            if (limit.HasValue)
            {
                args.Add("-l");
                args.Add(limit.Value.ToString());
            }

            if (itemSetDeduplication != ItemSetDeduplication.Default)
            {
                args.Add("-d");
                args.Add(itemSetDeduplication.GetDescription());
            }

            if (itemSetDescription != null)
            {
                args.Add("-r");
                args.Add(itemSetDescription);
            }

            if (deduplicateBy != DeduplicateBy.Individual)
            {
                args.Add("-b");
                args.Add(deduplicateBy.GetDescription());
            }

            if (custodianRanking != null)
            {
                args.Add("-c");
                args.Add(string.Join(",", custodianRanking));
            }

            var result = RunScript(nuixConsoleExePath, scriptPath, useDongle, args);

            await foreach (var line in result)
                yield return line;
        }


        /// <summary>
        /// Add items to a production set in NUIX
        /// </summary>
        /// <param name="nuixConsoleExePath">Path to the console exe</param>
        /// <param name="casePath">Where to create the new case</param>
        /// <param name="useDongle">Use a dongle for licensing</param>
        /// <param name="searchTerm">The term to search for</param>
        /// <param name="productionSetName">The production set to add the found items to. Will be created if it doesn't exist</param>
        /// <param name="order">Order by term e.g. name ASC</param>
        /// <param name="limit">Optional maximum number of items to tag.</param>
        /// <returns>The output of the case creation script</returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> AddToProductionSet( 

            string casePath= @"C:\Dev\Nuix\Cases\NewCase",
            string searchTerm = "night",
            string productionSetName  = "ProdSet",
            string? order = null,
            int? limit = null,
            string nuixConsoleExePath = @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe",
            bool useDongle = true)
        {
            var (searchTermParseSuccess, searchTermParseError, searchTermParsed) = Search.SearchParser.TryParse(searchTerm);

            if (!searchTermParseSuccess || searchTermParsed == null)
            {
                yield return new ResultLine(false, "Error parsing search term");
                if(searchTermParseError != null)
                    yield return new ResultLine(false, searchTermParseError);
                yield break;
            }

            if (searchTermParsed.AsString != searchTerm)
            {
                yield return new ResultLine(true, $"Search term simplified to '{searchTermParsed.AsString}'");
            }

            var currentDirectory = Directory.GetCurrentDirectory();
            var scriptPath = Path.Combine(currentDirectory, "..", "NuixClient", "Scripts", "AddToProductionSet.rb");

            var args = new List<string>
            {
                "-p", casePath,
                "-s", searchTermParsed.AsString,
                "-n", productionSetName,
                
            };
            if (order != null)
            {
                args.Add("-o ");
                args.Add(order);
            }

            if (limit.HasValue)
            {
                args.Add("-l");
                args.Add(limit.Value.ToString());
            }

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
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> CreateCase(
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

        /// <summary>
        /// Add file or folder to a Case in NUIX
        /// </summary>
        /// <param name="nuixConsoleExePath">Path to the console exe</param>
        /// <param name="casePath">Path of the case to open</param>
        /// <param name="folderName">The name of the folder to create</param>
        /// <param name="description">Description of the new folder</param>
        /// <param name="custodian">Custodian for the new folder</param>
        /// <param name="filePath">The path of the file to add</param>
        /// <param name="processingProfileName">The name of the processing profile to use. Can be null</param>
        /// <param name="useDongle">Use a dongle for licensing</param>
        /// <returns>The output of the case creation script</returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> AddFileToCase( //TODO remove default arguments

            string casePath = @"C:\Dev\Nuix\Cases\NewCase",
            string folderName = "TestFolder",
            string description = "nice",
            string custodian = "mark2",
            string filePath = @"C:\Dev\Nuix\Data\Custodians\BobS\Report3.ufdr",
            string? processingProfileName = null,
            string nuixConsoleExePath = @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe",
            bool useDongle = true)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var scriptPath = Path.Combine(currentDirectory, "..", "NuixClient", "Scripts", "AddToCase.rb");

            var args = new[]
            {
                "-p", casePath,
                "-n", folderName,
                "-d", description,
                "-c", custodian,
                "-f", filePath,
                "-r", processingProfileName
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
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> AddConcordanceToCase( //TODO remove default arguments

            string casePath = @"C:\Dev\Nuix\Cases\NewCase",
            string folderName = "BestFolder",
            string description = "nice",
            string custodian = "mw",
            string filePath = @"C:\Dev\Nuix\Exports\Export1\loadfile.dat",
            string concordanceDateFormat = @"yyyy-MM-dd'T'HH:mm:ss.SSSZ",
            string concordanceProfileName = @"TestProfile",
            string nuixConsoleExePath = @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe",
            
            bool useDongle = true)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var scriptPath = Path.Combine(currentDirectory, "..", "NuixClient", "Scripts", "AddConcordanceToCase.rb");

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
        /// <returns>The output of the case creation script</returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> ExportProductionSetConcordance( //TODO remove default arguments
            
            string casePath = @"C:\Dev\Nuix\Cases\NewCase",
            string exportPath = @"C:\Dev\Nuix\Exports\Export6",
            string productionSetName = @"Night",
            string metadataProfileName = "Default",
            string nuixConsoleExePath = @"C:\Program Files\Nuix\Nuix 7.8\nuix_console.exe",
            bool useDongle = true)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var scriptPath = Path.Combine(currentDirectory, "..", "NuixClient", "Scripts", "ExportConcordanceProcess.rb");

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