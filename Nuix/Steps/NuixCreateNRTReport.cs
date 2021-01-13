using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{

/// <summary>
/// Creates a report using an NRT file.
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
    log 'Generating NRT Report:'

    reportGenerator =$utilities.getReportGenerator();
    reportContext = {
    'NUIX_USER' => 'Mark',
    'NUIX_APP_NAME' => 'AppName',
    'NUIX_REPORT_TITLE' => 'ReportTitle',
    'NUIX_APP_VERSION' => NUIX_VERSION,
    'LOCAL_RESOURCES_URL' => localResourcesUrlArg,
    'currentCase' => $currentCase,
    'utilities' => $utilities,
    'dedupeEnabled' => true
    }

    reportGenerator.generateReport(
    nrtPathArg,
    reportContext.to_java,
    outputFormatArg,
    outputPathArg
    )";
}

/// <summary>
/// Creates a report using an NRT file.
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
    public IStep<StringStream> NRTPath { get; set; } = null!;

    /// <summary>
    /// The format of the report file that will be created.
    /// </summary>
    [Required]
    [Example("PDF")]
    [StepProperty(2)]
    [RubyArgument("outputFormatArg")]
    [Alias("Format")]
    public IStep<StringStream> OutputFormat { get; set; } = null!;

    /// <summary>
    /// The path to output the file at.
    /// </summary>
    [Required]
    [Example("C:/Temp/report.pdf")]
    [StepProperty(3)]
    [RubyArgument("outputPathArg")]
    [Alias("ReportPath")]
    public IStep<StringStream> OutputPath { get; set; } = null!;

    /// <summary>
    /// The path to the local resources folder.
    /// To load the logos etc.
    /// </summary>
    [Required]
    [Example(@"C:\Program Files\Nuix\Nuix 8.4\user-data\Reports\Case Summary\Resources\")]
    [StepProperty(4)]
    [RubyArgument("localResourcesUrlArg")]
    [Alias("Resources")]
    public IStep<StringStream> LocalResourcesURL { get; set; } = null!;
}

}
