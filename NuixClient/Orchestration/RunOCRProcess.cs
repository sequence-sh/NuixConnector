using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.Orchestration
{
    internal class RunOCRProcess : RubyScriptProcess
    {
        public override string GetName() => "RunOCR";

        /// <summary>
        /// The path to the case
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }


        /// <summary>
        /// The name of the OCR profile to use. If not provided, the default profile will be used
        /// </summary>
        [DataMember]
        [YamlMember(Order = 3)]
        public string? OCRProfileName { get; set; }


        internal override IEnumerable<string> GetArgumentErrors()
        {
            if(OCRProfileName != null)
            {
                yield return "Unfortunately OCRProfiles don't work because of a bug in NUIX";
            }            
        }
        internal override string ScriptName => "RunOCR.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            if(OCRProfileName != null)
                yield return ("-o", OCRProfileName);
        }
    }
}