﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Performs optical character recognition on files in a NUIX case.
    /// This is a compatibility version for users of Nuix versions prior to 7.6
    /// </summary>
    public sealed class NuixCompatibilityPerformOCR : RubyScriptProcess
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
        
        [YamlMember(Order = 3)]
        [ExampleValue("C:/Cases/MyCase")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }

        /// <summary>
        /// The term to use for searching for files to OCR.
        /// </summary>
        [Required]
        
        [YamlMember(Order = 3)]
        public string SearchTerm { get; set; } =
            "NOT flag:encrypted AND ((mime-type:application/pdf AND NOT content:*) OR (mime-type:image/* AND ( flag:text_not_indexed OR content:( NOT * ) )))";

        /// <summary>
        /// The name of the OCR profile to use.
        /// </summary>
        
        [YamlMember(Order = 3)]
        [DefaultValueExplanation("The default profile will be used.")]
        [ExampleValue("MyOcrProfile")]
        public string? OCRProfileName { get; set; }
        

        /// <inheritdoc />
        internal override string ScriptText => @"
    the_case = utilities.case_factory.open(pathArg)

    searchTerm = searchTermArg    
    items = the_case.searchUnsorted(searchTerm).to_a

    puts ""Running OCR on #{items.length} items""
    
    processor = utilities.createOcrProcessor() #since Nuix 7.0

    if ocrProfileArg != nil
        ocrOptions = {:ocrProfileName => ocrProfileArg}
        processor.process(items, profile)
        puts ""Items Processed""
    else
        processor.process(items)
        puts ""Items Processed""
    end    
    the_case.close";

        /// <inheritdoc />
        internal override string MethodName => "RunOCRCompatibly";

        /// <inheritdoc />
        internal override Version RequiredVersion { get; } = new Version(6,2);

        /// <inheritdoc />
        internal override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>
        {
            NuixFeature.OCR_PROCESSING
        };

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("searchTermArg", SearchTerm, false);
            yield return ("ocrProfileArg", OCRProfileName, true);
        }
    }
}