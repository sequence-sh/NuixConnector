namespace Reductech.Sequence.Connectors.Nuix.Steps
{

/// <summary>
/// Adds data from a Concordance file to a NUIX case.
/// </summary>
public sealed class NuixAddConcordanceFactory : RubyScriptStepFactory<NuixAddConcordance, Unit>
{
    private NuixAddConcordanceFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixAddConcordance, Unit> Instance { get; } =
        new NuixAddConcordanceFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } =
        new(7, 6); //This is required for the Metadata Import Profile

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature> { NuixFeature.CASE_CREATION, NuixFeature.METADATA_IMPORT };

    /// <inheritdoc />
    public override string FunctionName { get; } = "AddConcordanceToCase";

    /// <inheritdoc />
    public override string RubyFunctionText { get; } = @"
    processor = $current_case.create_processor

    if processingSettingsArg.nil?
      processor.set_processing_settings({
        'create_thumbnails' => false,
        'additional_digests' => [ 'SHA-1' ]
      })
    else
      processor.set_processing_settings(processingSettingsArg)
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

    load_options = {
      :concordanceFile => filePathArg,
      :concordanceDateFormat => dateFormatArg.nil? ? ""yyyy-MM-dd'T'HH:mm:ss.SSSZ"" : dateFormatArg
    }

    unless opticonPathArg.nil?
      log(""Adding opticon file: '#{opticonPathArg}'"", severity: :debug)
      load_options['useOpticonFile'] = true
	  load_options['opticonFile'] = opticonPathArg
    end
    
    container.addLoadFile(load_options)
    container.setMetadataImportProfileName(profileNameArg)
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
/// Adds data from a Concordance file to a NUIX case.
/// </summary>
[Alias("NuixImportConcordance")]
public sealed class NuixAddConcordance : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixAddConcordanceFactory.Instance;

    //TODO add a profile from a file - there is no Nuix function to do this right now.

    /// <summary>
    /// The path of the concordance file to import.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [Example("C:/MyConcordance.dat")]
    [RubyArgument("filePathArg")]
    [Alias("ConcordanceFile")]
    public IStep<StringStream> FilePath { get; set; } = null!;

    /// <summary>
    /// The name of the evidence container to add the concordance file to.
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("containerNameArg")]
    [Alias("ToContainer")]
    [Alias("FolderName")]
    public IStep<StringStream> Container { get; set; } = null!;

    /// <summary>
    /// The name of the concordance profile to use.
    /// </summary>
    [Required]
    [StepProperty(3)]
    [Example("MyProfile")]
    [RubyArgument("profileNameArg")]
    public IStep<StringStream> ConcordanceProfileName { get; set; } = null!;

    /// <summary>
    /// The concordance date format to use.
    /// </summary>
    [StepProperty]
    [RubyArgument("dateFormatArg")]
    [DefaultValueExplanation("yyyy-MM-dd'T'HH:mm:ss.SSSZ")]
    [Alias("DateFormat")]
    public IStep<StringStream>? ConcordanceDateFormat { get; set; }

    /// <summary>
    /// Path to the opticon file
    /// </summary>
    [StepProperty]
    [RubyArgument("opticonPathArg")]
    [DefaultValueExplanation("No opticon file will be processed")]
    [Alias("OpticonFile")]
    public IStep<StringStream>? OpticonPath { get; set; }

    /// <summary>
    /// The description of the evidence container.
    /// </summary>
    [StepProperty]
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
    /// Sets the processing settings to use.
    /// These settings correspond to the same settings in the desktop application,
    /// however the user's preferences are not used to derive the defaults.
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("(create_thumbnails: false, additional_digests: ['SHA-1'])")]
    [RubyArgument("processingSettingsArg")]
    [Alias("Settings")]
    public IStep<Entity>? ProcessingSettings { get; set; }

    /// <summary>
    /// The number of items at which the Nuix processor logs a progress message.
    /// </summary>
    [StepProperty]
    [RubyArgument("progressIntervalArg")]
    [DefaultValueExplanation("Every 5000 items")]
    public IStep<SCLInt> ProgressInterval { get; set; } = new SCLConstant<SCLInt>(5000.ConvertToSCLObject());

    /// <summary>
    /// Sets additional metadata on the evidence container.
    /// </summary>
    [StepProperty]
    [RubyArgument("customMetadataArg")]
    [DefaultValueExplanation("No custom metadata will be added")]
    public IStep<Entity>? CustomMetadata { get; set; }

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
    [Alias("TimeZone")]
    public IStep<StringStream>? ContainerTimeZone { get; set; }
}

}
