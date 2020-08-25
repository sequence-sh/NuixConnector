using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;

namespace Reductech.EDR.Connectors.Nuix.processes
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
        public override Version RequiredVersion { get; } = new Version(7, 4);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
            new List<NuixFeature> { NuixFeature.ANALYSIS };
    }

    /// <summary>
    /// Creates a report using an NRT file.
    /// </summary>
    public sealed class NuixCreateNRTReport : RubyScriptProcess
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixCreateNRTReportProcessFactory.Instance;


        ///// <inheritdoc />
        //public override string GetName() => "Create NRT Report";

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]

        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        public string CasePath { get; set; }

        /// <summary>
        /// The NRT file path.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        public string NRTPath { get; set; }

        /// <summary>
        /// The format of the report file that will be created.
        /// </summary>
        [Required]
        [Example("PDF")]
        [RunnableProcessProperty]
        public string OutputFormat { get; set; }

        /// <summary>
        /// The path to the local resources folder.
        /// To load the logo's etc.
        /// </summary>
        [Required]
        [Example(@"C:\Program Files\Nuix\Nuix 8.4\user-data\Reports\Case Summary\Resources\")]
        [RunnableProcessProperty]
        public string LocalResourcesURL { get; set; }

        /// <summary>
        /// The path to output the file at.
        /// </summary>
        [Required]
        [Example("C:/Temp/report.pdf")]
        [RunnableProcessProperty]
        public string OutputPath { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        internal override string ScriptText =>
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

        /// <inheritdoc />
        internal override string MethodName => "CreateNRTReport";

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("nrtPathArg", NRTPath, false);
            yield return ("outputFormatArg", OutputFormat, false);
            yield return ("outputPathArg", OutputPath, false);
            yield return ("localResourcesUrlArg", LocalResourcesURL, false);
        }
    }
}