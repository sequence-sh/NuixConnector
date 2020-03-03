using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using NuixSearch;
using Processes.process;
using YamlDotNet.Serialization;

namespace NuixClient.processes
{

    /// <summary>
    /// A process that runs a ruby script in Nuix and also writes something to a file on the file system
    /// </summary>
    public abstract class RubyScriptWithOutputProcess : RubyScriptProcess
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

        private static readonly Regex HexRegex = new Regex(@"\A(?:0x)(?:[0-9a-f]{2}\s?)+\Z", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal override async void OnScriptFinish(ProcessState processState)
        {
            foreach (var (fileName, lines) in processState.Artifacts)
            {
                var filePath = Path.Combine(OutputFolder, fileName + ".txt");
                await using var fileStream = File.CreateText(filePath);

                foreach (var line in (List<string>) lines)
                {
                    var terms = line.Split('\t');

                    var newString =
                        string.Join('\t',
                            terms.Select(MaybeConvertFromHex));

                    fileStream.WriteLine(newString);
                }
            }
            base.OnScriptFinish(processState);

            static string MaybeConvertFromHex(string term) => HexRegex.IsMatch(term) ? FromHexString(term.Substring(2)) : term;
            static string FromHexString(string hexString)
            {
                var bytes = new byte[hexString.Length / 2];
                for (var i = 0; i < bytes.Length; i++)
                {
                    bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                }

                return Encoding.UTF8.GetString(bytes);
            }
        }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
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