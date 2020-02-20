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
            yield break;
        }
        internal override string ScriptName => "RunOCR.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            if(OCRProfileName != null)
                yield return ("-o", OCRProfileName);
        }
    }


    /// <summary>
    /// A process which creates a new case
    /// </summary>
    internal class CreateCaseProcess : RubyScriptProcess
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName() => $"Create Case '{CaseName}'";


        /// <summary>
        /// The name of the case to create
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CaseName { get; set; }


        /// <summary>
        /// The path to the folder to create the case in
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 4)]
        public string CasePath { get; set; }

        /// <summary>
        /// Name of the investigator
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 5)]
        public string Investigator { get; set; }

        /// <summary>
        /// Description of the case
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 6)]
        public string Description { get; set; }


#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        internal override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
        }

        internal override string ScriptName => "CreateCase.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-n", CaseName);
            yield return ("-d", Description);
            yield return ("-i", Investigator);
        }
    }
}