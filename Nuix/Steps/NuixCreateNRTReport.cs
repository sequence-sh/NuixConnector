using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Creates a report using an NRT file.
    /// </summary>
    public sealed class NuixCreateNRTReportStepFactory : RubyScriptStepFactory<NuixCreateNRTReport, Unit>
    {
        private NuixCreateNRTReportStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<NuixCreateNRTReport, Unit> Instance { get; } = new NuixCreateNRTReportStepFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(7, 4);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
            new List<NuixFeature> { NuixFeature.ANALYSIS };

        /// <inheritdoc />
        public override string FunctionName => "CreateNRTReport";

        /// <inheritdoc />
        public override string RubyFunctionText =>
            @"
    the_case = $utilities.case_factory.open(pathArg)
    log 'Generating NRT Report:'

    reportGenerator =$utilities.getReportGenerator();
    reportContext = {
    'NUIX_USER' => 'Mark',
    'NUIX_APP_NAME' => 'AppName',
    'NUIX_REPORT_TITLE' => 'ReportTitle',
    'NUIX_APP_VERSION' => NUIX_VERSION,
    'LOCAL_RESOURCES_URL' => localResourcesUrlArg,
    'currentCase' => the_case,
    'utilities' => $utilities,
    'dedupeEnabled' => true
    }

    reportGenerator.generateReport(
    nrtPathArg,
    reportContext.to_java,
    outputFormatArg,
    outputPathArg
    )

    the_case.close";
    }

    /// <summary>
    /// Creates a report using an NRT file.
    /// </summary>
    public sealed class NuixCreateNRTReport : RubyScriptStepBase<Unit>
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => NuixCreateNRTReportStepFactory.Instance;


        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [StepProperty(1)]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IStep<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The NRT file path.
        /// </summary>
        [Required]
        [StepProperty(2)]
        [RubyArgument("nrtPathArg", 2)]
        public IStep<string> NRTPath { get; set; } = null!;

        /// <summary>
        /// The format of the report file that will be created.
        /// </summary>
        [Required]
        [Example("PDF")]
        [StepProperty(3)]
        [RubyArgument("outputFormatArg", 3)]
        public IStep<string> OutputFormat { get; set; } = null!;

        /// <summary>
        /// The path to output the file at.
        /// </summary>
        [Required]
        [Example("C:/Temp/report.pdf")]
        [StepProperty(4)]
        [RubyArgument("outputPathArg", 4)]
        public IStep<string> OutputPath { get; set; } = null!;

        /// <summary>
        /// The path to the local resources folder.
        /// To load the logo's etc.
        /// </summary>
        [Required]
        [Example(@"C:\Program Files\Nuix\Nuix 8.4\user-data\Reports\Case Summary\Resources\")]
        [StepProperty(5)]
        [RubyArgument("localResourcesUrlArg", 5)]
        public IStep<string> LocalResourcesURL { get; set; } = null!;
    }
}