using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.processes
{
    internal class NuixNRTReport : RubyScriptProcess
    {
        public override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
        }

        public override string GetName() => "Create NRT Report";

        internal override string ScriptName => "CreateNRTReport.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-n", NRTPath);
            yield return ("-o", OutputPath);
            yield return ("-f", OutputFormat);
        }

        /// <summary>
        /// The path to the case to create the report from
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }


        /// <summary>
        /// The path to the NRT file
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 4)]
        public string NRTPath { get; set; }

        /// <summary>
        /// Where to output the report
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 5)]
        public string OutputPath { get; set; }

        /// <summary>
        /// The output format e.g. PDF or HTML
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 6)]
        public string OutputFormat { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}