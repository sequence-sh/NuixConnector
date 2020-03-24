using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    internal class ImmutableRubyScriptWithOutputProcess : ImmutableRubyScriptProcess
    {
        /// <inheritdoc />
        public ImmutableRubyScriptWithOutputProcess(string name, string nuixExeConsolePath, IReadOnlyCollection<string> arguments, string outputFolder) : base(name, nuixExeConsolePath, arguments)
        {
            _outputFolder = outputFolder;
        }

        private readonly string _outputFolder;


        private static readonly Regex HexRegex = new Regex(@"\A(?:0x)(?:[0-9a-f]{2}\s?)+\Z", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal override IEnumerable<Result<string>> OnScriptFinish(ProcessState processState)
        {
            foreach (var (fileName, lines) in processState.Artifacts)
            {
                var filePath = Path.Combine(_outputFolder, fileName + ".txt");
                using var fileStream = File.CreateText(filePath);

                foreach (var line in (List<string>) lines)
                {
                    var terms = line.Split('\t');

                    var newString =
                        string.Join('\t',
                            terms.Select(MaybeConvertFromHex));

                    fileStream.WriteLine(newString);
                }
            }
            return base.OnScriptFinish(processState);

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
    }
}