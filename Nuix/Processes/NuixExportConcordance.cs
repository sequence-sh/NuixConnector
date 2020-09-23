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
    /// Exports Concordance for a particular production set.
    /// </summary>
    public sealed class NuixExportConcordanceProcessFactory : RubyScriptProcessFactory<NuixExportConcordance, Unit>
    {
        private NuixExportConcordanceProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixExportConcordance, Unit> Instance { get; } = new NuixExportConcordanceProcessFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(7, 2); //I'm checking the production profile here

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET,
            NuixFeature.EXPORT_ITEMS
        };

        /// <inheritdoc />
        public override string FunctionName => "ExportConcordance";

        /// <inheritdoc />
        public override string RubyFunctionText =>
            @"
    the_case = utilities.case_factory.open(pathArg)

    productionSet = the_case.findProductionSetByName(productionSetNameArg)

    if productionSet == nil
        puts ""Could not find production set with name '#{productionSetNameArg.to_s}'""
    elsif productionSet.getProductionProfile == nil
        puts ""Production set '#{productionSetNameArg.to_s}' did not have a production profile set.""
    else
        batchExporter = utilities.createBatchExporter(exportPathArg)


        puts 'Starting export.'
        batchExporter.exportItems(productionSet)
        puts 'Export complete.'

    end

    the_case.close";
    }


    /// <summary>
    /// Exports Concordance for a particular production set.
    /// </summary>
    public sealed class NuixExportConcordance : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<Unit> RubyScriptProcessFactory => NuixExportConcordanceProcessFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>

        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IRunnableProcess<string> CasePath { get; set; } = null!;

        /// <summary>
        /// Where to export the Concordance to.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [RubyArgument("exportPathArg", 2)]
        public IRunnableProcess<string> ExportPath { get; set; }= null!;

        /// <summary>
        /// The name of the production set to export.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [RubyArgument("productionSetNameArg", 3)]
        public IRunnableProcess<string> ProductionSetName { get; set; }= null!;

    }
}
