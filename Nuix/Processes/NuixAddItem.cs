using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Adds a file or directory to a Nuix Case.
    /// </summary>
    public sealed class NuixAddItemProcessFactory : RubyScriptProcessFactory<NuixAddItem, Unit>
    {
        private NuixAddItemProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixAddItem, Unit> Instance { get; } = new NuixAddItemProcessFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion => new Version(3, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature> { NuixFeature.CASE_CREATION };


        /// <inheritdoc />
        public override string FunctionName => "AddToCase";

        /// <inheritdoc />
        public override string RubyFunctionText => @"
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

    }

    /// <summary>
    /// Adds a file or directory to a Nuix Case.
    /// </summary>
    public sealed class NuixAddItem : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<Unit> RubyScriptProcessFactory => NuixAddItemProcessFactory.Instance;



        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IRunnableProcess<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The name of the folder to create.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [RubyArgument("folderNameArg", 2)]
        public IRunnableProcess<string> FolderName { get; set; } = null!;

        /// <summary>
        /// The description of the new folder.
        /// </summary>
        [RunnableProcessProperty]
        [RubyArgument("folderDescriptionArg", 3)]
        public IRunnableProcess<string>? Description { get; set; }

        /// <summary>
        /// The custodian to assign to the new folder.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [RubyArgument("folderCustodianArg", 4)]
        public IRunnableProcess<string> Custodian { get; set; } = null!;



        /// <summary>
        /// The path of the file or directory to add to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Data/File.txt")]
        [RubyArgument("filePathArg", 5)]
        public IRunnableProcess<string> Path { get; set; } = null!;

        /// <summary>
        /// The name of the Processing profile to use.
        /// </summary>

        [RequiredVersion("Nuix", "7.6")]
        [RunnableProcessProperty]
        [Example("MyProcessingProfile")]
        [DefaultValueExplanation("The default processing profile will be used.")]
        [RubyArgument("processingProfileNameArg", 6)]
        public IRunnableProcess<string>? ProcessingProfileName { get; set; }

        /// <summary>
        /// The path to the Processing profile to use
        /// </summary>
        [RequiredVersion("Nuix", "7.6")]
        [RunnableProcessProperty]
        [Example("C:/Profiles/MyProcessingProfile.xml")]
        [DefaultValueExplanation("The default processing profile will be used.")]
        [RubyArgument("processingProfilePathArg", 7)]
        public IRunnableProcess<string>? ProcessingProfilePath { get; set; }


        /// <summary>
        /// The path of a file containing passwords to use for decryption.
        /// </summary>
        [RequiredVersion("Nuix", "7.6")]
        [RunnableProcessProperty]
        [Example("C:/Data/Passwords.txt")]
        [RubyArgument("passwordFilePathArg", 8)]
        public IRunnableProcess<string>? PasswordFilePath { get; set; }

        /// <inheritdoc />
        public override Version? RunTimeNuixVersion
        {
            get
            {
                if (ProcessingProfilePath != null || ProcessingProfileName != null)
                    return new Version(7, 6);
                if (PasswordFilePath != null)
                    return new Version(7, 6);
                return null;
            }
        }

        /// <inheritdoc />
        public override Result<Unit, IRunErrors> VerifyThis
        {
            get
            {
                if (ProcessingProfileName != null && ProcessingProfilePath != null)
                {
                    return new RunError(
                        $"Only one of {nameof(ProcessingProfileName)} and {nameof(ProcessingProfilePath)} may be set.",
                        Name,
                        null,
                        ErrorCode.ConflictingParameters);
                }

                return Unit.Default;
            }
        }

    }
}