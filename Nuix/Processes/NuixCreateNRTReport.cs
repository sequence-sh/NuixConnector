using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Creates a report using an NRT file.
    /// </summary>
    public sealed class NuixCreateNRTReport : RubyScriptProcess
    {
        /// <inheritdoc />
        public override string GetName() => "Create NRT Report";

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        
        [YamlMember(Order = 3)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }

        /// <summary>
        /// The NRT file path.
        /// </summary>
        [Required]
        [YamlMember(Order = 4)]
        public string NRTPath { get; set; }

        /// <summary>
        /// The format of the report file that will be created.
        /// </summary>
        [Required]
        [ExampleValue("PDF")]
        [YamlMember(Order = 5)]
        public string OutputFormat { get; set; }

        /// <summary>
        /// The path to the local resources folder.
        /// To load the logo's etc.
        /// </summary>
        [Required]
        [ExampleValue(@"C:\Program Files\Nuix\Nuix 8.4\user-data\Reports\Case Summary\Resources\")]
        [YamlMember(Order = 6)]
        public string LocalResourcesURL { get; set; }

        /// <summary>
        /// The path to output the file at.
        /// </summary>
        [Required]
        [ExampleValue("C:/Temp/report.pdf")]
        [YamlMember(Order = 7)]
        public string OutputPath { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        internal override string ScriptText =>
            @"the_case = utilities.case_factory.open(pathArg)
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
    NRTPathArg,
    reportContext.to_java,
    OutputFormatArg,
    OutputPathArg
    )

    the_case.close";

        /// <inheritdoc />
        internal override string MethodName => "CreateNRTReport";

        /// <inheritdoc />
        internal override IEnumerable<(string arg, string? val, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("NRTPathArg", NRTPath, false);
            yield return ("OutputFormatArg", OutputFormat, false);
            yield return ("OutputPathArg", OutputPath, false);
            yield return ("localResourcesUrlArg", LocalResourcesURL, false);
        }
    }
}