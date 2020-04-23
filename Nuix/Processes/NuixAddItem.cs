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
    /// Adds a file or directory to a Nuix Case.
    /// </summary>
    public sealed class NuixAddItem : RubyScriptProcess
    {

        /// <inheritdoc />
        protected override NuixReturnType ReturnType => NuixReturnType.Unit;

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
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        
        /// <summary>
        /// The path of a file containing passwords to use for decryption.
        /// </summary>
        [RequiredVersion("Nuix", "7.6")]
        [Required]
        [YamlMember(Order = 8)]
        [ExampleValue("C:/Data/Passwords.txt")]
        public string? PasswordFilePath { get; set; }


        /// <summary>
        /// The name of the Processing profile to use.
        /// </summary>
        
        [RequiredVersion("Nuix", "7.6")]
        [YamlMember(Order = 9)]
        [ExampleValue("MyProcessingProfile")]
        [DefaultValueExplanation("The default processing profile will be used.")]
        public string? ProcessingProfileName { get; set; }

        /// <summary>
        /// The path to the Processing profile to use
        /// </summary>
        [RequiredVersion("Nuix", "7.6")]
        [YamlMember(Order = 10)]
        [ExampleValue("C:/Profiles/MyProcessingProfile.xml")]
        [DefaultValueExplanation("The default processing profile will be used.")]
        public string? ProcessingProfilePath { get; set; }



        /// <inheritdoc />
        internal override string ScriptText => @"
    the_case = utilities.case_factory.open(pathArg)
    processor = the_case.create_processor

#This only works in 7.6 or later
    if processingProfileNameArg != nil
        processor.setProcessingProfile(processingProfileNameArg) 
    elsif processingProfilePathArg != nil
        profileBuilder = utilities.getProcessingProfileBuilder()
        profileBuilder.load(processingProfilePathArg)
        profile = profileBuilder.build()

        if profile == nil
            puts ""Could not find processing profile at #{processingProfilePathArg}""
            exit
        end

        processor.setProcessingProfileObject(profile)
    end


#This only works in 7.2 or later
    if passwordFilePathArg != nil
        lines = File.read(passwordFilePathArg, mode: 'r:bom|utf-8').split

        passwords = lines.map {|p| p.chars.to_java(:char)}
        listName = 'MyPasswordList'

        processor.addPasswordList(listName, passwords)
        processor.setPasswordDiscoverySettings({'mode' => ""word-list"", 'word-list' => listName })
    end


    folder = processor.new_evidence_container(folderNameArg)

    folder.description = folderDescriptionArg if folderDescriptionArg != nil
    folder.initial_custodian = folderCustodianArg

    folder.add_file(filePathArg)
    folder.save

    puts 'Adding items'
    processor.process
    puts 'Items added'
    the_case.close";

        /// <inheritdoc />
        internal override string MethodName => "AddToCase";

        /// <inheritdoc />
        internal override Version RequiredVersion
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(ProcessingProfilePath) || !string.IsNullOrWhiteSpace(ProcessingProfileName))
                    return new Version(7, 6);
                if (!string.IsNullOrWhiteSpace(PasswordFilePath))
                    return new Version(7, 6);

                return new Version(3,2);
            }
        }

        /// <inheritdoc />
        internal override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>{NuixFeature.CASE_CREATION};

        /// <inheritdoc />
        internal override IEnumerable<string> GetAdditionalArgumentErrors()
        {
            if(ProcessingProfileName != null && ProcessingProfilePath != null)
                yield return $"Only one of {nameof(ProcessingProfileName)} and {nameof(ProcessingProfilePath)} may be set.";
        }

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("folderNameArg", FolderName, false);
            yield return ("folderDescriptionArg", Description, true);
            yield return ("folderCustodianArg", Custodian, false);
            yield return ("filePathArg", Path, false);
            yield return ("processingProfileNameArg", ProcessingProfileName, true);
            yield return ("processingProfilePathArg", ProcessingProfilePath, true);
            yield return ("passwordFilePathArg", PasswordFilePath, true);
        }
    }
}