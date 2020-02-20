﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using NuixClient.Search;
using YamlDotNet.Serialization;

namespace NuixClient.Orchestration
{
    internal abstract class RubyScriptWithOutputProcess : RubyScriptProcess
    {

        private readonly IDictionary<string, List<string>> _files = new Dictionary<string, List<string>>();

        private static readonly Regex OutputLineRegex = new Regex(@"\AOutput\s*(?<filename>[\w-]+)\s*:(?<data>.*)\Z", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        internal override bool HandleLine(ResultLine rl)
        {
            if (!rl.IsSuccess)
                return true;

            if (OutputLineRegex.TryMatch(rl.Line, out var match))
            {
                var fileName = match.Groups["filename"].Value;
                var data = match.Groups["data"].Value;

                //This is an output line - it will be written to a file
                if (_files.TryGetValue(fileName, out var lines))
                    lines.Add(data);
                else
                    _files.Add(fileName, new List<string> { data });

                return false; //don't display line
            }
            else return base.HandleLine(rl);
        }

        internal override async void OnScriptFinish()
        {
            foreach (var (fileName, lines) in _files)
            {
                var filePath = Path.Combine(OutputFolder, fileName + ".txt");
                await using var fileStream = File.CreateText(filePath);

                foreach (var line in lines)
                    fileStream.WriteLine(line);
            }
            base.OnScriptFinish();
        }

        internal override IEnumerable<string> GetArgumentErrors()
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