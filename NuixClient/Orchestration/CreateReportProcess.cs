using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using NuixClient.Search;
using YamlDotNet.Serialization;

namespace NuixClient.Orchestration
{
    internal class CreateReportProcess : RubyScriptProcess
    {
        public override string GetName() => "Create Report";

        /// <summary>
        /// The path to the folder to create the case in
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 4)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }

        /// <summary>
        /// The path to the folder to put the output files in
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 5)]
        public string OutputFolder { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        internal override IEnumerable<string> GetArgumentErrors()
        {
            if (string.IsNullOrWhiteSpace(OutputFolder))
                yield return $"{nameof(OutputFolder)} field must not be empty.";

            if(!Directory.Exists(OutputFolder))
                yield return $"{OutputFolder} does not exist.";
        }

        internal override string ScriptName => "CreateReport.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
        }


        private bool _hasFailed;
        private readonly IDictionary<string, List<string>> _files = new Dictionary<string, List<string>>();

        private static readonly Regex OutputLineRegex = new Regex(@"\AOutput\s*(?<filename>\w+)\s*:(?<data>.*)\Z", RegexOptions.Compiled| RegexOptions.IgnoreCase);

        internal override bool HandleLine(ResultLine rl)
        {
            if (!rl.IsSuccess)
            {
                _hasFailed = true;
                return true;
            }

            if (OutputLineRegex.TryMatch(rl.Line, out var match))
            {
                var fileName = match.Groups["filename"].Value;
                var data = match.Groups["data"].Value;
                
                //This is an output line - it will be written to a file
                if (_files.TryGetValue(fileName, out var lines))
                    lines.Add(data);
                else
                    _files.Add(fileName, new List<string> {data});

                return false; //don't display line
            }
            else return base.HandleLine(rl);
        }

        internal override async void OnScriptFinish()
        {
            if (!_hasFailed)
            {
                foreach (var (fileName, lines) in _files)
                {
                    var filePath = Path.Combine(OutputFolder, fileName);
                    await using var fileStream = File.CreateText(filePath);

                    foreach (var line in lines)
                        fileStream.WriteLine(line);
                }
            }

            base.OnScriptFinish();
        }
    }
}