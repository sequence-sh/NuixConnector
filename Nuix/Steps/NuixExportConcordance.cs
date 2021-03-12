using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{

/// <summary>
/// Exports Concordance for a particular production set.
/// </summary>
public sealed class
    NuixExportConcordanceStepFactory : RubyScriptStepFactory<NuixExportConcordance, Unit>
{
    private NuixExportConcordanceStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixExportConcordance, Unit> Instance { get; } =
        new NuixExportConcordanceStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } =
        new(7, 2); // Minimum version required to use a production set

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>() { NuixFeature.PRODUCTION_SET, NuixFeature.EXPORT_ITEMS };

    /// <inheritdoc />
    public override string FunctionName => "ExportConcordance";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    production_set = $current_case.findProductionSetByName(productionSetNameArg)

    if productionSet.nil?
      write_error(""Could not find production set '#{productionSetNameArg}'"", terminating: true)
    end

    if productionSet == nil
      log ""Could not find production set '#{productionSetNameArg}'""
    elsif productionSet.getProductionProfile == nil
      log ""Production set '#{productionSetNameArg.to_s}' did not have a production profile set.""
    end

    exporter = $utilities.create_batch_exporter(exportPathArg)

    traversal_options = {}
    unless traversalStrategyArg.nil?
      log(""Traversal strategy: '#{traversalStrategyArg}'"", severity: :debug)
      traversal_options[:strategy] = traversalStrategyArg
    end
    unless deduplicationArg.nil?
      log(""Traversal deduplication: '#{deduplicationArg}'"", severity: :debug)
      traversal_options[:deduplication] = deduplicationArg
    end
    unless sortArg.nil?
      log(""Traversal sort order: '#{sortArg}'"", severity: :debug)
      traversal_options[:sortOrder] = sortArg
    end
    unless exportDescendantContainersArg.nil?
      log(""Traversal export descendant containers: '#{exportDescendantContainersArg}'"", severity: :debug)
      traversal_options[:exportDescendantContainers] = exportDescendantContainersArg
    end
    exporter.set_traversal_options(traversal_options) unless traversal_options.empty?
    exporter.setSkipNativesSlipsheetedItems(skipSlipsheetedItemsArg) unless skipSlipsheetedItemsArg.nil?
    exporter.setNumberingOptions(numberingOptionsArg) unless numberingOptionsArg.nil?
    exporter.before_export { log 'Starting export' }
    exporter.export_items(production_set)
    log 'Export finished'
";
}

/// <summary>
/// Exports a production set in Concorfance format
/// </summary>
public sealed class NuixExportConcordance : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixExportConcordanceStepFactory.Instance;

    /// <summary>
    /// The directory where the export will be created
    /// </summary>
    [Required]
    [StepProperty(1)]
    [RubyArgument("exportPathArg")]
    [Alias("ToDirectory")]
    public IStep<StringStream> ExportPath { get; set; } = null!;

    /// <summary>
    /// The name of the production set to export.
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("productionSetNameArg")]
    [Alias("ProductionSet")]
    public IStep<StringStream> ProductionSetName { get; set; } = null!;

    /// <summary>
    /// Set the numbering options for the export.
    /// This setting has no effect if the production set has numbering options defined.
    /// See Nuix API <code>NumberingConfigurable.setNumberingOptions()</code>
    /// for more details on the available options.
    /// </summary>
    [StepProperty]
    [RubyArgument("numberingOptionsArg")]
    [DefaultValueExplanation("Document ID numbering, starting with DOC-000000001")]
    [Example("(createProductionSet: false prefix: 'ABC' documentId: (startAt: 1 minWidth: 4))")]
    public IStep<Core.Entity>? NumberingOptions { get; set; }

    /// <summary>
    /// The method of selecting which items to export.
    /// </summary>
    [StepProperty]
    [RubyArgument("traversalStrategyArg")]
    [DefaultValueExplanation(nameof(ExportTraversalStrategy.Items))]
    public IStep<ExportTraversalStrategy>? TraversalStrategy { get; set; }

    /// <summary>
    /// Method of deduplication when top-level item export is used.
    /// </summary>
    [StepProperty]
    [RubyArgument("deduplicationArg")]
    [DefaultValueExplanation(nameof(ExportDeduplication.None))]
    public IStep<ExportDeduplication>? Deduplication { get; set; }

    /// <summary>
    /// Method of sorting items during the export.
    /// </summary>
    [StepProperty]
    [RubyArgument("sortArg")]
    [DefaultValueExplanation(nameof(ExportSortOrder.None))]
    public IStep<ExportSortOrder>? SortOrder { get; set; }

    /// <summary>
    /// Export descendant containers.
    /// Only works when TraversalStrategy includes descendants.
    /// </summary>
    [StepProperty]
    [RubyArgument("exportDescendantContainersArg")]
    [DefaultValueExplanation("false")]
    public IStep<bool>? ExportDescendantContainers { get; set; }

    /// <summary>
    /// Skip export of slipsheeted items.
    /// </summary>
    [StepProperty]
    [RequiredVersion("Nuix", "7.6")]
    [RubyArgument("skipSlipsheetedItemsArg")]
    [DefaultValueExplanation("false")]
    public IStep<bool>? SkipSlipsheetedNatives { get; set; }
}

}
