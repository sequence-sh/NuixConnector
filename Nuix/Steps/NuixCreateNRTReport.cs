namespace Sequence.Connectors.Nuix.Steps;

/// <summary>
/// Create a case report using an NRT file.
/// </summary>
public sealed class
    NuixCreateNRTReportStepFactory : RubyScriptStepFactory<NuixCreateNRTReport, Unit>
{
    private NuixCreateNRTReportStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixCreateNRTReport, Unit> Instance { get; } =
        new NuixCreateNRTReportStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(7, 4);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature> { NuixFeature.ANALYSIS };

    /// <inheritdoc />
    public override string FunctionName => "CreateNRTReport";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    context = {
      'NUIX_USER' => userArg.nil? ? $current_case.get_investigator : userArg,
      'NUIX_APP_NAME' => appNameArg.nil? ? 'Nuix' : appNameArg,
      'NUIX_REPORT_TITLE' => titleArg.nil? ? ""#{$current_case.get_name} Report"" : titleArg,
      'NUIX_APP_VERSION' => NUIX_VERSION,
      'currentCase' => $current_case,
      'utilities' => $utilities,
      'dedupeEnabled' => true
    }
    context['LOCAL_RESOURCES_URL'] = localResourcesUrlArg.nil? ?
      nrtPathArg.sub(/(?i)\.nrt$/,'') + '\\resources\\' :
      localResourcesUrlArg
    context['GLOBAL_RESOURCES_URL'] = globalResourcesUrlArg unless globalResourcesUrlArg.nil?
    log(""Report context: #{context}"", severity: :trace)
    $utilities.get_report_generator.generate_report(
      nrtPathArg,
      context.to_java,
      outputFormatArg,
      outputPathArg
    )
";
}

/// <summary>
/// Create a case report using an NRT file.
/// </summary>
public sealed class NuixCreateNRTReport : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixCreateNRTReportStepFactory.Instance;

    /// <summary>
    /// The NRT file path.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [RubyArgument("nrtPathArg")]
    [Alias("Template")]
    public IStep<StringStream> NRTPath { get; set; } = null!;

    /// <summary>
    /// The output path.
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("outputPathArg")]
    [Example("C:/Temp/report.pdf")]
    [Alias("ReportPath")]
    public IStep<StringStream> OutputPath { get; set; } = null!;

    /// <summary>
    /// The format of the report file that will be created.
    /// </summary>
    [StepProperty(3)]
    [RubyArgument("outputFormatArg")]
    [DefaultValueExplanation("PDF")]
    [Alias("Format")]
    public IStep<StringStream> OutputFormat { get; set; } = new SCLConstant<StringStream>("PDF");

    /// <summary>
    /// The report title.
    /// </summary>
    [StepProperty]
    [RubyArgument("titleArg")]
    [DefaultValueExplanation("<Case Name> Report")]
    [Alias("ReportTitle")]
    [Alias("NUIX_REPORT_TITLE")]
    public IStep<StringStream>? Title { get; set; }

    /// <summary>
    /// The report user.
    /// </summary>
    [StepProperty]
    [RubyArgument("userArg")]
    [DefaultValueExplanation("The case investigator property.")]
    [Alias("NuixUser")]
    [Alias("NUIX_USER")]
    public IStep<StringStream>? User { get; set; }

    /// <summary>
    /// The application name generating the report.
    /// </summary>
    [StepProperty]
    [RubyArgument("appNameArg")]
    [DefaultValueExplanation("Nuix")]
    [Alias("NuixAppName")]
    [Alias("NUIX_APP_NAME")]
    public IStep<StringStream>? ApplicationName { get; set; }

    /// <summary>
    /// The path to the local resources folder. Must have a trailing '\'.
    /// The resources folder can be obtained by unzipping the NRT file.
    /// </summary>
    [StepProperty]
    [RubyArgument("localResourcesUrlArg")]
    [Example(@"C:\ProgramData\Nuix\Reports\Resources\")]
    [DefaultValueExplanation("NRTPath with extension removed and \\resources appended.")]
    [Alias("LocalResourcesUrl")]
    [Alias("LOCAL_RESOURCES_URL")]
    public IStep<StringStream>? LocalResourcesPath { get; set; }

    /// <summary>
    /// The path to the global resources folder. Must have a trailing '\'.
    /// </summary>
    [StepProperty]
    [RubyArgument("globalResourcesUrlArg")]
    [DefaultValueExplanation("No global resource path set")]
    [Alias("GlobalResourcesUrl")]
    [Alias("GLOBAL_RESOURCES_URL")]
    public IStep<StringStream>? GlobalResourcesPath { get; set; }
}
