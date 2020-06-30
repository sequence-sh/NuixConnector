using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Performs optical character recognition on files in a NUIX case.
    /// </summary>
    public sealed class NuixPerformOCR : RubyScriptProcess
    {
        /// <inheritdoc />
        protected override NuixReturnType ReturnType => NuixReturnType.Unit;

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => "RunOCR";

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        
        [YamlMember(Order = 1)]
        [ExampleValue("C:/Cases/MyCase")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }

        /// <summary>
        /// The term to use for searching for files to OCR.
        /// </summary>
        [Required]
        
        [YamlMember(Order = 2)]
        public string SearchTerm { get; set; } =
            "NOT flag:encrypted AND ((mime-type:application/pdf AND NOT content:*) OR (mime-type:image/* AND ( flag:text_not_indexed OR content:( NOT * ) )))";

        /// <summary>
        /// The name of the OCR profile to use.
        /// This cannot be set at the same time as OCRProfilePath.
        /// </summary>
        [YamlMember(Order = 3)]
        [DefaultValueExplanation("The default profile will be used.")]
        [ExampleValue("MyOcrProfile")]
        public string? OCRProfileName { get; set; }

        /// <summary>
        /// Path to the OCR profile to use.
        /// This cannot be set at the same times as OCRProfileName.
        /// </summary>
        [YamlMember(Order = 4)]
        [RequiredVersion("Nuix", "7.6")]
        [DefaultValueExplanation("The default profile will be used.")]
        [ExampleValue("C:\\Profiles\\MyProfile.xml")]
        public string? OCRProfilePath { get; set; }
        

        /// <inheritdoc />
        internal override string ScriptText => @"
    the_case = utilities.case_factory.open(pathArg)

    searchTerm = searchTermArg    
    items = the_case.searchUnsorted(searchTerm).to_a

    puts ""Running OCR on #{items.length} items""
    
    processor = utilities.createOcrProcessor() #since Nuix 7.0 but seems to work with earlier versions anyway

    if ocrProfileArg != nil
        ocrOptions = {:ocrProfileName => ocrProfileArg}
        processor.process(items, ocrOptions)
        puts ""Items Processed""
    elsif ocrProfilePathArg != nil
        profileBuilder = utilities.getOcrProfileBuilder()
        profile = profileBuilder.load(ocrProfilePathArg)

        if profile == nil
            puts ""Could not find processing profile at #{ocrProfilePathArg}""
            exit
        end        

        processor.setOcrProfileObject(profile)
    else
        processor.process(items)
        puts ""Items Processed""
    end    
    the_case.close";

        /// <inheritdoc />
        internal override string MethodName => "RunOCR";

        /// <inheritdoc />`,
        internal override Version RequiredVersion
        {
            get
            {
                if(!string.IsNullOrWhiteSpace(OCRProfilePath) || !string.IsNullOrWhiteSpace(OCRProfileName))
                {
                    return new Version(7, 6);
                }
                return new Version(7, 6);
            }
        }

        /// <inheritdoc />
        internal override IEnumerable<string> GetAdditionalArgumentErrors()
        {
            if(OCRProfileName != null && OCRProfilePath != null)
                yield return $"Only one of {nameof(OCRProfileName)} and {nameof(OCRProfilePath)} may be set.";
        }


        /// <inheritdoc />
        internal override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.OCR_PROCESSING
        };

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("searchTermArg", SearchTerm, false);
            yield return ("ocrProfileArg", OCRProfileName, true);
            yield return ("ocrProfilePathArg", OCRProfilePath, true);
        }
    }
}