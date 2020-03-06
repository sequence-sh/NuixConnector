using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.processes
{
    /// <summary>
    /// Creates a report using an NRT file.
    /// </summary>
    public sealed class NuixCreateNRTReport : RubyScriptProcess
    {
        internal override string ScriptName => "CreateNRTReport.rb";

        /// <inheritdoc />
        public override string GetName() => "Create NRT Report";

        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-n", NRTPAth);
            yield return ("-f", OutputFormat);
            yield return ("-o", OutputPath);
            yield return ("-l", LocalResourcesURL);
            
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetArgumentErrors()
        {
            yield break; //TODO
        }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The case folder path.
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 3)]
        public string CasePath { get; set; }

        /// <summary>
        /// The NRT file path
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 4)]
        public string NRTPAth { get; set; }

        /// <summary>
        /// The format of the report file that will be created.
        /// e.g. PDF or HTML
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 5)]
        public string OutputFormat { get; set; }

        /// <summary>
        /// The path to the local resources folder.
        /// To load the logo's etc.
        /// e.g. C:\Program Files\Nuix\Nuix 8.4\user-data\Reports\Case Summary\resources\
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 6)]
        public string LocalResourcesURL { get; set; }

        /// <summary>
        /// The path to output the file at. 
        /// e.g. C:/Temp/report.pdf
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 7)]
        public string OutputPath { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    }
}