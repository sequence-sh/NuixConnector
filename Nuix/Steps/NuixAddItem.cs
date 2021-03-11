using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{

/// <summary>
/// Adds a file or directory to a Nuix Case.
/// </summary>
public sealed class NuixAddItemStepFactory : RubyScriptStepFactory<NuixAddItem, Unit>
{
    private NuixAddItemStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixAddItem, Unit> Instance { get; } =
        new NuixAddItemStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion => new(3, 2);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature> { NuixFeature.CASE_CREATION };

    /// <inheritdoc />
    public override string FunctionName => "AddToCase";

    /// <inheritdoc />
    public override string RubyFunctionText => @"

    ds = args['datastream']

    processor = $current_case.create_processor

    #Read special mime type settings from data stream
    if ds != nil
      log 'Mime Type Data stream reading started'
      mimeTypes = []

      while !ds.closed? or !ds.empty?
        data = ds.pop
        break if ds.closed? and data.nil?
        mimeTypes << data
      end
      log ""Mime Type Data stream reading finished (#{mimeTypes.count} elements)""

      version_mimes = []
      $utilities.getItemTypeUtility().getAllTypes().each do |mime|
        version_mimes << mime.to_s
      end

      mimeTypes.each do |mime_type|
        mimeTypeString = mime_type['mimeType'].to_s
        if (version_mimes.include?(mimeTypeString) == true)
          mime_type.delete('mimeType') #remove this value from the hash as it isn't part of the settings
          nuix_processor.setMimeTypeProcessingSettings(mimeTypeString, mime_type)
        end
      end
    end

    #This only works in 7.6 or later
    if processingProfileNameArg != nil
      processor.setProcessingProfile(processingProfileNameArg)
    elsif processingProfilePathArg != nil
      profileBuilder = $utilities.getProcessingProfileBuilder()
      profileBuilder.load(processingProfilePathArg)
      profile = profileBuilder.build()

      if profile == nil
        raise ""Could not find processing profile at #{processingProfilePathArg}""
        exit
      end

      processor.setProcessingProfileObject(profile)
    end

    if processingSettingsArg != nil
      processor.setProcessingSettings(processingSettingsArg)
    end

    if parallelProcessingSettingsArg != nil
      processor.setParallelProcessingSettings(parallelProcessingSettingsArg)
    end

    #This only works in 7.2 or later
    if passwordFilePathArg != nil
      lines = File.read(passwordFilePathArg, mode: 'r:bom|utf-8').split
      passwords = lines.map {|p| p.chars.to_java(:char)}
      listName = 'MyPasswordList'
      processor.addPasswordList(listName, passwords)
      processor.setPasswordDiscoverySettings({'mode' => 'word-list', 'word-list' => listName })
    end

    log ""Creating new evidence container '#{folderNameArg}'""
    folder = processor.new_evidence_container(folderNameArg)

    log(""Container description: #{folderDescriptionArg}"", severity: :trace)
    folder.description = folderDescriptionArg if folderDescriptionArg != nil

    unless folderCustodianArg.nil?
      log(""Container custodian: '#{folderCustodianArg}'"", severity: :trace)
      folder.initial_custodian = folderCustodianArg
    end

    unless customMetadataArg.nil?
      log(""Adding custom metadata to container #{folderNameArg}"", severity: :debug)
      folder.set_custom_metadata(customMetadataArg)
    end

    filePathsArgs.each do |path|
      folder.add_file(path)
      log ""Adding to Container: #{folderNameArg} Path: #{path}""
    end

    folder.save

	processor.when_cleaning_up do
      log 'Processor cleaning up'
	end

    semaphore = Mutex.new
    item_count = 0

    processor.when_item_processed do |item|
	  semaphore.synchronize do
		item_count += 1
        log(""Processor items processed: #{item_count}"") if item_count % progressIntervalArg == 0
      end
    end

    log 'Processor starting'
    processor.process
    log ""Processor finished. Total items processed: #{item_count}""
";
}

/// <summary>
/// Adds a file or directory to a Nuix Case.
/// </summary>
[Alias("NuixAdd")]
[Alias("NuixImportItem")]
public sealed class NuixAddItem : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixAddItemStepFactory.Instance;

    /// <summary>
    /// The path of the file or directory to add to the case.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [Example("C:/Data/File.txt")]
    [RubyArgument("filePathsArgs")]
    [Alias("Directories")]
    [Alias("Files")]
    public IStep<Array<StringStream>> Paths { get; set; } = null!;

    /// <summary>
    /// The name of the folder to create.
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("folderNameArg")]
    [Alias("Container")]
    [Alias("ToContainer")]
    public IStep<StringStream> FolderName { get; set; } = null!;

    /// <summary>
    /// The description of the new folder.
    /// </summary>
    [StepProperty(3)]
    [RubyArgument("folderDescriptionArg")]
    [DefaultValueExplanation("No Description")]
    public IStep<StringStream>? Description { get; set; }

    /// <summary>
    /// The custodian to assign to the new folder/container.
    /// </summary>
    [StepProperty(4)]
    [RubyArgument("folderCustodianArg")]
    public IStep<StringStream> Custodian { get; set; } = null!;

    /// <summary>
    /// The name of the Processing profile to use.
    /// </summary>
    [RequiredVersion("Nuix", "7.6")]
    [StepProperty(5)]
    [Example("MyProcessingProfile")]
    [DefaultValueExplanation("The default processing profile will be used.")]
    [RubyArgument("processingProfileNameArg")]
    [Alias("UsingProfile")]
    public IStep<StringStream>? ProcessingProfileName { get; set; }

    /// <summary>
    /// The path to the Processing profile to use
    /// </summary>
    [RequiredVersion("Nuix", "7.6")]
    [StepProperty(6)]
    [Example("C:/Profiles/MyProcessingProfile.xml")]
    [DefaultValueExplanation("The default processing profile will be used.")]
    [RubyArgument("processingProfilePathArg")]
    [Alias("UsingProfilePath")]
    public IStep<StringStream>? ProcessingProfilePath { get; set; }

    /// <summary>
    /// Sets the processing settings to use.
    /// These settings correspond to the same settings in the desktop application,
    /// however the user's preferences are not used to derive the defaults.
    /// </summary>
    [StepProperty(7)]
    [DefaultValueExplanation("Processing settings will not be changed")]
    [RubyArgument("processingSettingsArg")]
    [Alias("Settings")]
    public IStep<Core.Entity>? ProcessingSettings { get; set; }

    /// <summary>
    /// Sets the parallel processing settings to use.
    /// These settings correspond to the same settings in the desktop application,
    /// however the user's preferences are not used to derive the defaults.
    /// </summary>
    [StepProperty(8)]
    [DefaultValueExplanation("Parallel processing settings will not be changed")]
    [RubyArgument("parallelProcessingSettingsArg")]
    public IStep<Core.Entity>? ParallelProcessingSettings { get; set; }

    /// <summary>
    /// The path of a file containing passwords to use for decryption.
    /// </summary>
    [RequiredVersion("Nuix", "7.6")]
    [StepProperty(9)]
    [Example("C:/Data/Passwords.txt")]
    [RubyArgument("passwordFilePathArg")]
    [DefaultValueExplanation("Do not attempt decryption")]
    [Alias("PasswordFile")]
    public IStep<StringStream>? PasswordFilePath { get; set; }

    /// <summary>
    /// Special settings for individual mime types.
    /// Should have a 'mime_type' property and then any other special properties.
    /// </summary>
    [RequiredVersion("Nuix", "8.2")]
    [StepProperty(10)]
    [RubyArgument("mimeTypeDataStreamArg")]
    [DefaultValueExplanation("Use default settings for all MIME types")]
    public IStep<Array<Core.Entity>>? MimeTypeSettings { get; set; }

    /// <summary>
    /// The number of items at which the Nuix processor logs a progress message.
    /// </summary>
    [StepProperty(11)]
    [RubyArgument("progressIntervalArg")]
    [DefaultValueExplanation("Every 5000 items")]
    public IStep<int> ProgressInterval { get; set; } = new IntConstant(5000);

    /// <summary>
    /// Sets additional metadata on the folder/container.
    /// </summary>
    [StepProperty(12)]
    [RubyArgument("customMetadataArg")]
    [DefaultValueExplanation("No custom metadata will be added")]
    public IStep<Core.Entity>? CustomMetadata { get; set; }

    /// <inheritdoc />
    public override Result<Unit, IError> VerifyThis(SCLSettings settings)
    {
        if (ProcessingProfileName != null && ProcessingProfilePath != null)
        {
            return new SingleError(
                new ErrorLocation(this),
                ErrorCode.ConflictingParameters,
                nameof(ProcessingProfileName),
                nameof(ProcessingProfilePath)
            );
        }

        return base.VerifyThis(settings);
    }
}

}
