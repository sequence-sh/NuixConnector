using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Adds data from a Concordance file to a NUIX case.
    /// </summary>
    public sealed class NuixAddConcordanceFactory : RubyScriptProcessFactory<NuixAddConcordance,Unit>
    {
        private NuixAddConcordanceFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixAddConcordance, Unit> Instance { get; } = new NuixAddConcordanceFactory();

        /// <inheritdoc />
        public override Version RequiredVersion { get; } = new Version(7, 6); //This is required for the Metadata Import Profile

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature> { NuixFeature.CASE_CREATION, NuixFeature.METADATA_IMPORT };

    }



    /// <summary>
    /// Adds data from a Concordance file to a NUIX case.
    /// </summary>
    public sealed class NuixAddConcordance : RubyScriptProcess
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixAddConcordanceFactory.Instance;



        ///// <inheritdoc />
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public override string GetName()
        //{
        //    return "Add Concordance";
        //}

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The name of the concordance profile to use.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("MyProfile")]
        public IRunnableProcess<string> ConcordanceProfileName { get; set; }

        //TODO add a profile from a file - there is no Nuix function to do this right now.

        /// <summary>
        /// The concordance date format to use.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("yyyy-MM-dd'T'HH:mm:ss.SSSZ")]
        public IRunnableProcess<string> ConcordanceDateFormat { get; set; }

        /// <summary>
        /// The path of the concordance file to import.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/MyConcordance.dat")]
        public IRunnableProcess<string> FilePath { get; set; }

        /// <summary>
        /// The name of the custodian to assign the folder to.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        public IRunnableProcess<string> Custodian { get; set; }

        /// <summary>
        /// A description to add to the folder.
        /// </summary>
        [RunnableProcessProperty]
        public IRunnableProcess<string>? Description { get; set; }

        /// <summary>
        /// The name of the folder to create.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        public IRunnableProcess<string> FolderName { get; set; }

        /// <summary>
        /// The path to the case to import into.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        public IRunnableProcess<string> CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        internal override string ScriptText =>
//language=RUBY
@"
    the_case = utilities.case_factory.open(pathArg)
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

    puts 'Starting processing.'
    processor.process
    puts 'Processing complete.'
    the_case.close";



        /// <inheritdoc />
        internal override string MethodName => "AddConcordanceToCase";

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, IRunnableProcess? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("folderNameArg", FolderName, false);
            yield return ("folderDescriptionArg", Description, true);
            yield return ("folderCustodianArg", Custodian, false);
            yield return ("filePathArg", FilePath, false);
            yield return ("dateFormatArg", ConcordanceDateFormat, false);
            yield return ("profileNameArg", ConcordanceProfileName, false);
        }
    }
}