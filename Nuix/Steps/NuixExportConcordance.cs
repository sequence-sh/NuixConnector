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
    /// Exports Concordance for a particular production set.
    /// </summary>
    public sealed class NuixExportConcordanceStepFactory : RubyScriptStepFactory<NuixExportConcordance, Unit>
    {
        private NuixExportConcordanceStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<NuixExportConcordance, Unit> Instance { get; } = new NuixExportConcordanceStepFactory();

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
    public sealed class NuixExportConcordance : RubyScriptStepBase<Unit>
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => NuixExportConcordanceStepFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>

        [Required]
        [StepProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IStep<string> CasePath { get; set; } = null!;

        /// <summary>
        /// Where to export the Concordance to.
        /// </summary>
        [Required]
        [StepProperty]
        [RubyArgument("exportPathArg", 2)]
        public IStep<string> ExportPath { get; set; }= null!;

        /// <summary>
        /// The name of the production set to export.
        /// </summary>
        [Required]
        [StepProperty]
        [RubyArgument("productionSetNameArg", 3)]
        public IStep<string> ProductionSetName { get; set; }= null!;

    }
}
