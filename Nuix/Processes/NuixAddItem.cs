using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.Processes
{
    /// <summary>
    /// Adds a file or directory to a Nuix Case.
    /// </summary>
    public sealed class NuixAddItem : RubyScriptProcess
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => $"Add '{Path}'";

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// The path of the file or directory to add to the case.
        /// </summary>
        [Required]
        [YamlMember( Order = 3)]
        [ExampleValue("C:/Data/File.txt")]
        public string Path { get; set; }

        /// <summary>
        /// The custodian to assign to the new folder.
        /// </summary>
        [Required]
        [YamlMember(Order = 4)]
        public string Custodian { get; set; }

        /// <summary>
        /// The description of the new folder.
        /// </summary>
        [YamlMember(Order = 5)]
        public string? Description { get; set; }

        /// <summary>
        /// The name of the folder to create.
        /// </summary>
        [Required]
        [YamlMember(Order = 6)]
        public string FolderName { get; set; }

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [YamlMember(Order = 7)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }


        /// <summary>
        /// The name of the processing profile to use.
        /// </summary>
        
        [YamlMember(Order = 7)]
        [ExampleValue("MyProcessingProfile")]
        [DefaultValueExplanation("The default processing profile will be used.")]
        public string? ProcessingProfileName { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        internal override string ScriptText => @"the_case = utilities.case_factory.open(pathArg)
    processor = the_case.create_processor
    processor.setProcessingProfile(processingProfileNameArg) if processingProfileNameArg != nil

    folder = processor.new_evidence_container(folderNameArg)

    folder.description = folderDescriptionArg if folderDescriptionArg != nil
    folder.initial_custodian = folderCustodianArg

    folder.add_file(filePathArg)
    folder.save

    puts 'Starting processing.'
    processor.process
    puts 'Processing complete.'
    the_case.close";

        /// <inheritdoc />
        internal override string MethodName => "AddToCase";

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("folderNameArg", FolderName, false);
            yield return ("folderDescriptionArg", Description, true);
            yield return ("folderCustodianArg", Custodian, false);
            yield return ("filePathArg", Path, false);
            yield return ("processingProfileNameArg", ProcessingProfileName, true);
        }
    }
}