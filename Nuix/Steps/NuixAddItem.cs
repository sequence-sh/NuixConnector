using CSharpFunctionalExtensions;
using Reductech.Sequence.Core.Internal.Errors;

namespace Reductech.Sequence.Connectors.Nuix.Steps;

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
    processor = $current_case.create_processor

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

    ds = args['datastream']

    #Read special mime type settings from data stream
    unless ds.nil?
      log 'Mime Type Data stream reading started'
      mime_types = []

      while !ds.closed? or !ds.empty?
        data = ds.pop
        break if ds.closed? and data.nil?
        mime_types << JSON.parse(data)
      end
      log ""Mime Type Data stream reading finished (#{mime_types.count} entities)""

      supported_mimes = []
      $utilities.get_item_type_utility.get_all_types.each do |mime|
        supported_mimes << mime.to_s
      end
      log(""Current Nuix version supports #{supported_mimes.count} mime types"", severity: :debug)

      mime_types.each do |mt|
        type_name = mt['mimeType']
        if supported_mimes.include?(type_name)
          mt.delete('mimeType') # remove this value from the hash as it isn't part of the settings
          log(""Settings for '#{type_name}': #{mt}"", severity: :debug)
          processor.setMimeTypeProcessingSettings(type_name, mt)
        else
          log(""The mime type #{type_name} is not supported by the current Nuix version"", severity: :warn)
        end
      end
    end

    #This only works in 7.2 or later
    if passwordFilePathArg != nil
      lines = File.read(passwordFilePathArg, mode: 'r:bom|utf-8').split
      passwords = lines.map {|p| p.chars.to_java(:char)}
      listName = 'MyPasswordList'
      processor.addPasswordList(listName, passwords)
      processor.setPasswordDiscoverySettings({'mode' => 'word-list', 'word-list' => listName })
    end

    log ""Creating new evidence container '#{containerNameArg}'""
    container = processor.new_evidence_container(containerNameArg)

    unless containerDescriptionArg.nil?
      log(""Container description: #{containerDescriptionArg}"", severity: :debug)
      container.description = containerDescriptionArg
    end

    unless containerCustodianArg.nil?
      log(""Container custodian: '#{containerCustodianArg}'"", severity: :debug)
      container.initial_custodian = containerCustodianArg
    end

    unless customMetadataArg.nil?
      log(""Adding custom metadata to container #{containerNameArg}"", severity: :debug)
      container.set_custom_metadata(customMetadataArg)
    end

    unless containerEncodingArg.nil?
      log(""Container encoding: '#{containerEncodingArg}'"", severity: :debug)
      container.set_encoding(containerEncodingArg)
    end

    unless containerLocaleArg.nil?
      log(""Container locale: '#{containerLocaleArg}'"", severity: :debug)
      container.set_locale(containerLocaleArg)
    end

    unless containerTimeZoneArg.nil?
      log(""Container time zone: '#{containerTimeZoneArg}'"", severity: :debug)
      container.set_time_zone(containerTimeZoneArg)
    end

    filePathsArgs.each do |path|
      container.add_file(path)
      log ""Adding to Container: #{containerNameArg} Path: #{path}""
    end

    container.save

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
    /// The name of the evidence container to add items to.
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("containerNameArg")]
    [Alias("ToContainer")]
    public IStep<StringStream> Container { get; set; } = null!;

    /// <summary>
    /// The description of the evidence container.
    /// </summary>
    [StepProperty(3)]
    [RubyArgument("containerDescriptionArg")]
    [DefaultValueExplanation("No Description")]
    [Alias("ContainerDescription")]
    public IStep<StringStream>? Description { get; set; }

    /// <summary>
    /// The custodian to assign to the new evidence container.
    /// </summary>
    [StepProperty]
    [RubyArgument("containerCustodianArg")]
    [DefaultValueExplanation("No custodian assigned")]
    [Alias("ContainerCustodian")]
    public IStep<StringStream>? Custodian { get; set; }

    /// <summary>
    /// The name of the Processing profile to use.
    /// </summary>
    [RequiredVersion(NuixVersionKey, "7.6")]
    [StepProperty]
    [Example("MyProcessingProfile")]
    [DefaultValueExplanation("The default processing profile will be used.")]
    [RubyArgument("processingProfileNameArg")]
    [Alias("UsingProfile")]
    public IStep<StringStream>? ProcessingProfileName { get; set; }

    /// <summary>
    /// The path to the Processing profile to use
    /// </summary>
    [RequiredVersion(NuixVersionKey, "7.6")]
    [StepProperty]
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
    [StepProperty]
    [DefaultValueExplanation("Processing settings will not be changed")]
    [RubyArgument("processingSettingsArg")]
    [Alias("Settings")]
    public IStep<Core.Entity>? ProcessingSettings { get; set; }

    /// <summary>
    /// Sets the parallel processing settings to use.
    /// These settings correspond to the same settings in the desktop application,
    /// however the user's preferences are not used to derive the defaults.
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("Parallel processing settings will not be changed")]
    [RubyArgument("parallelProcessingSettingsArg")]
    public IStep<Core.Entity>? ParallelProcessingSettings { get; set; }

    /// <summary>
    /// The path of a file containing passwords to use for decryption.
    /// </summary>
    [RequiredVersion(NuixVersionKey, "7.6")]
    [StepProperty]
    [Example("C:/Data/Passwords.txt")]
    [RubyArgument("passwordFilePathArg")]
    [DefaultValueExplanation("Do not attempt decryption")]
    [Alias("PasswordFile")]
    public IStep<StringStream>? PasswordFilePath { get; set; }

    /// <summary>
    /// Special settings for individual mime types.
    /// Should have a 'mimeType' property and then any other special properties.
    /// </summary>
    [RequiredVersion(NuixVersionKey, "8.2")]
    [StepProperty]
    [RubyArgument("mimeTypeDataStreamArg")]
    [DefaultValueExplanation("Use default settings for all MIME types")]
    [Example(
        "(\"mimeType\": \"application/pdf\" \"enabled\": \"true\" \"processEmbedded\": false)"
    )]
    public IStep<Array<Core.Entity>>? MimeTypeSettings { get; set; }

    /// <summary>
    /// The number of items at which the Nuix processor logs a progress message.
    /// </summary>
    [StepProperty]
    [RubyArgument("progressIntervalArg")]
    [DefaultValueExplanation("Every 5000 items")]
    public IStep<SCLInt> ProgressInterval { get; set; } =
        new SCLConstant<SCLInt>(5000.ConvertToSCLObject());

    /// <summary>
    /// Sets additional metadata on the evidence container.
    /// </summary>
    [StepProperty]
    [RubyArgument("customMetadataArg")]
    [DefaultValueExplanation("No custom metadata will be added")]
    public IStep<Core.Entity>? CustomMetadata { get; set; }

    /// <summary>
    /// Set the encoding for the evidence container.
    /// </summary>
    [StepProperty]
    [Example("UTF-8")]
    [RubyArgument("containerEncodingArg")]
    [DefaultValueExplanation("Default system encoding")]
    [Alias("Encoding")]
    public IStep<StringStream>? ContainerEncoding { get; set; }

    /// <summary>
    /// Set the locale for the evidence container.
    /// </summary>
    [RequiredVersion(NuixVersionKey, "7.2")]
    [StepProperty]
    [Example("en-GB")]
    [RubyArgument("containerLocaleArg")]
    [DefaultValueExplanation("Default system locale")]
    [Alias("Locale")]
    public IStep<StringStream>? ContainerLocale { get; set; }

    /// <summary>
    /// Set the time zone for the evidence container.
    /// If the time zone given is not known to Nuix, the GMT time zone will be used.
    /// </summary>
    [StepProperty]
    [Example("UTC")]
    [RubyArgument("containerTimeZoneArg")]
    [DefaultValueExplanation("Default system time zone")]
    [RequiredVersion("NuixVersion", "0.9.0", "11.0")]
    [RequiredFeature("NuixFeatures", "Case_Creation")]
    [Alias("TimeZone")]
    public IStep<StringStream>? ContainerTimeZone { get; set; }

    /// <inheritdoc />
    public override Result<Unit, IError> VerifyThis(StepFactoryStore stepFactoryStore)
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

        return base.VerifyThis(stepFactoryStore);
    }
}
