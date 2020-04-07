using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Adds data from a Concordance file to a NUIX case.
    /// </summary>
    public sealed class NuixAddConcordance : RubyScriptProcess
    {
        /// <inheritdoc />
        protected override NuixReturnType ReturnType => NuixReturnType.Unit;

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName()
        {
            return "Add Concordance";
        }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The name of the concordance profile to use.
        /// </summary>
        [Required]
        [YamlMember(Order = 3)]
        [ExampleValue("MyProfile")]
        public string ConcordanceProfileName { get; set; }

        /// <summary>
        /// The concordance date format to use.
        /// </summary>
        [Required]
        [YamlMember(Order = 4)]
        [ExampleValue("yyyy-MM-dd'T'HH:mm:ss.SSSZ")]
        public string ConcordanceDateFormat { get; set; }

        /// <summary>
        /// The path of the concordance file to import.
        /// </summary>
        [Required]
        [YamlMember(Order = 5)]
        [ExampleValue("C:/MyConcordance.dat")]
        public string FilePath { get; set; }

        /// <summary>
        /// The name of the custodian to assign the folder to.
        /// </summary>
        [Required]
        [YamlMember(Order = 6)]
        public string Custodian { get; set; }

        /// <summary>
        /// A description to add to the folder.
        /// </summary>
        [YamlMember(Order = 7)]
        public string? Description { get; set; }

        /// <summary>
        /// The name of the folder to create.
        /// </summary>
        [Required]
        [YamlMember(Order = 8)]
        public string FolderName { get; set; }

        /// <summary>
        /// The path to the case to import into.
        /// </summary>
        [Required]
        [YamlMember(Order = 9)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }
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
        internal override Version RequiredVersion { get; } = new Version(7,6);

        /// <inheritdoc />
        internal override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>{NuixFeature.CASE_CREATION, NuixFeature.METADATA_IMPORT };


        /// <inheritdoc />
        internal override string MethodName => "AddConcordanceToCase";

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
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