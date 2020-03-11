using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Performs optical character recognition on files which need it in a NUIX case.
    /// </summary>
    public sealed class NuixPerformOCR : RubyScriptProcess
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => "RunOCR";

        /// <summary>
        /// The path to the case
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 3)]
        [ExampleValue("C:/Cases/MyCase")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }

        /// <summary>
        /// The term to use for searching for files to OCR.
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 3)]
        public string SearchTerm { get; set; } =
            "NOT flag:encrypted AND ((mime-type:application/pdf AND NOT content:*) OR (mime-type:image/* AND ( flag:text_not_indexed OR content:( NOT * ) )))";

        /// <summary>
        /// The name of the OCR profile to use.
        /// </summary>
        [DataMember]
        [YamlMember(Order = 3)]
        [DefaultValueExplanation("The default profile will be used.")]
        [ExampleValue("MyOcrProfile")]
        public string? OCRProfileName { get; set; }

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IEnumerable<string> GetArgumentErrors()
        {
            yield break;      
        }
        internal override string ScriptName => "RunOCR.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-s", SearchTerm);
            if(OCRProfileName != null)
                yield return ("-o", OCRProfileName);
        }
    }
}