using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Adds data from a Concordance file to a NUIX case.
    /// </summary>
    public sealed class NuixAddConcordanceFactory : RubyScriptStepFactory<NuixAddConcordance, Unit>
    {
        private NuixAddConcordanceFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<NuixAddConcordance, Unit> Instance { get; } =
            new NuixAddConcordanceFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(7, 6); //This is required for the Metadata Import Profile

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature> { NuixFeature.CASE_CREATION, NuixFeature.METADATA_IMPORT };


        /// <inheritdoc />
        public override string FunctionName { get; } = "AddConcordanceToCase";

        /// <inheritdoc />
        public override string RubyFunctionText { get; } =
//language=RUBY
            @"
    the_case =$utilities.case_factory.open(pathArg)
    processor = the_case.create_processor
    processor.processing_settings = { :create_thumbnails       => false,
                                    :additional_digests      => [ 'SHA-1' ] }


    folder = processor.new_evidence_container(folderNameArg)

    folder.description = folderDescriptionArg
    folder.initial_custodian = folderCustodianArg
    folder.addLoadFile({
    :concordanceFile => filePathArg,
    :concordanceDateFormat => dateFormatArg
    })
    folder.setMetadataImportProfileName(profileNameArg)
    folder.save

    log 'Starting processing.'
    processor.process
    log 'Processing complete.'
    the_case.close";
    }



    /// <summary>
    /// Adds data from a Concordance file to a NUIX case.
    /// </summary>
    public sealed class NuixAddConcordance : RubyScriptStepBase<Unit>
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => NuixAddConcordanceFactory.Instance;

        //TODO add a profile from a file - there is no Nuix function to do this right now.

        /// <summary>
        /// The path to the case to import into.
        /// </summary>
        [Required]
        [StepProperty(1)]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IStep<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The name of the folder to create.
        /// </summary>
        [Required]
        [StepProperty(2)]
        [RubyArgument("folderNameArg", 2)]
        public IStep<string> FolderName { get; set; } = null!;

        /// <summary>
        /// A description to add to the folder.
        /// </summary>
        [StepProperty(3)]
        [RubyArgument("folderDescriptionArg", 3)]
        [DefaultValueExplanation("No description")]
        public IStep<string>? Description { get; set; }

        /// <summary>
        /// The name of the custodian to assign the folder to.
        /// </summary>
        [Required]
        [StepProperty(4)]
        [RubyArgument("folderCustodianArg", 4)]
        public IStep<string> Custodian { get; set; } = null!;

        /// <summary>
        /// The path of the concordance file to import.
        /// </summary>
        [Required]
        [StepProperty(5)]
        [Example("C:/MyConcordance.dat")]
        [RubyArgument("filePathArg", 5)]
        public IStep<string> FilePath { get; set; } = null!;

        /// <summary>
        /// The concordance date format to use.
        /// </summary>
        [Required]
        [StepProperty(6)]
        [Example("yyyy-MM-dd'T'HH:mm:ss.SSSZ")]
        [RubyArgument("dateFormatArg", 6)]
        public IStep<string> ConcordanceDateFormat { get; set; } = null!;

        /// <summary>
        /// The name of the concordance profile to use.
        /// </summary>
        [Required]
        [StepProperty(7)]
        [Example("MyProfile")]
        [RubyArgument("profileNameArg", 7)]
        public IStep<string> ConcordanceProfileName { get; set; } = null!;
    }
}