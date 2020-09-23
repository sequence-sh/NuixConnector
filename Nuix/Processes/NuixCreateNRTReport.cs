using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Processes
{
    /// <summary>
    /// Creates a report using an NRT file.
    /// </summary>
    public sealed class NuixCreateNRTReportProcessFactory : RubyScriptProcessFactory<NuixCreateNRTReport, Unit>
    {
        private NuixCreateNRTReportProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixCreateNRTReport, Unit> Instance { get; } = new NuixCreateNRTReportProcessFactory();

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
    the_case = utilities.case_factory.open(pathArg)
    puts 'Generating NRT Report:'

    reportGenerator = utilities.getReportGenerator();
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
    public sealed class NuixCreateNRTReport : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<Unit> RubyScriptProcessFactory => NuixCreateNRTReportProcessFactory.Instance;


        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IRunnableProcess<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The NRT file path.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [RubyArgument("nrtPathArg", 2)]
        public IRunnableProcess<string> NRTPath { get; set; } = null!;

        /// <summary>
        /// The format of the report file that will be created.
        /// </summary>
        [Required]
        [Example("PDF")]
        [RunnableProcessProperty]
        [RubyArgument("outputFormatArg", 3)]
        public IRunnableProcess<string> OutputFormat { get; set; } = null!;

        /// <summary>
        /// The path to output the file at.
        /// </summary>
        [Required]
        [Example("C:/Temp/report.pdf")]
        [RunnableProcessProperty]
        [RubyArgument("outputPathArg", 4)]
        public IRunnableProcess<string> OutputPath { get; set; } = null!;

        /// <summary>
        /// The path to the local resources folder.
        /// To load the logo's etc.
        /// </summary>
        [Required]
        [Example(@"C:\Program Files\Nuix\Nuix 8.4\user-data\Reports\Case Summary\Resources\")]
        [RunnableProcessProperty]
        [RubyArgument("localResourcesUrlArg", 5)]
        public IRunnableProcess<string> LocalResourcesURL { get; set; } = null!;
    }
}