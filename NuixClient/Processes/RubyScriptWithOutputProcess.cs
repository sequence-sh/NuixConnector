using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using NuixClient.Search;
using Orchestration.Processes;
using YamlDotNet.Serialization;

namespace NuixClient.Processes
{
    internal abstract class RubyScriptWithOutputProcess : RubyScriptProcess
    {
        private static readonly Regex OutputLineRegex = new Regex(@"\AOutput\s*(?<filename>[\w-]+)\s*:(?<data>.*)\Z", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal override bool HandleLine(Result<string> rl, ProcessState processState)
        {
            if (rl.IsFailure)
                return true;

            if (OutputLineRegex.TryMatch(rl.Value, out var match))
            {
                var fileName = match.Groups["filename"].Value;
                var data = match.Groups["data"].Value;

                //This is an output line - it will be written to a file
                if (processState.Artifacts.TryGetValue(fileName, out var lines))
                    ((List<string>) lines).Add(data);
                else
                    processState.Artifacts.Add(fileName, new List<string> { data });

                return false; //don't display line
            }
            else return base.HandleLine(rl, processState);
        }

        internal override async void OnScriptFinish(ProcessState processState)
        {
            foreach (var (fileName, lines) in processState.Artifacts)
            {
                var filePath = Path.Combine(OutputFolder, fileName + ".txt");
                await using var fileStream = File.CreateText(filePath);

                foreach (var line in (List<string>) lines)
                    fileStream.WriteLine(line);
            }
            base.OnScriptFinish(processState);
        }

        public override IEnumerable<string> GetArgumentErrors()
        {
            if (string.IsNullOrWhiteSpace(OutputFolder))
                yield return $"{nameof(OutputFolder)} field must not be empty.";

            if (!Directory.Exists(OutputFolder))
                yield return $"{OutputFolder} does not exist.";
        }


        /// <summary>
        /// The path to the folder to put the output files in
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 4)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string OutputFolder { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}