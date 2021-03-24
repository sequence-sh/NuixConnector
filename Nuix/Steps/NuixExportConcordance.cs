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
    if production_set.nil?
      write_error(""Could not find production set '#{productionSetNameArg}'"", terminating: true)
    end
    prod_profile = production_set.get_production_profile
    if exportOptionsArg.nil? and prod_profile.nil?
      write_error('No ExportOptions or Production Profile has been set.', terminating: true)
    end
    traversal_options = {}
    unless traversalStrategyArg.nil?
      log(""Traversal strategy: #{traversalStrategyArg}"", severity: :debug)
      traversal_options[:strategy] = traversalStrategyArg
    end
    unless deduplicationArg.nil?
      log(""Traversal deduplication: #{deduplicationArg}"", severity: :debug)
      traversal_options[:deduplication] = deduplicationArg
    end
    unless sortArg.nil?
      log(""Traversal sort order: #{sortArg}"", severity: :debug)
      traversal_options[:sortOrder] = sortArg
    end
    unless exportDescendantContainersArg.nil?
      log(""Traversal export descendant containers: #{exportDescendantContainersArg}"", severity: :debug)
      traversal_options[:exportDescendantContainers] = exportDescendantContainersArg
    end
    exporter = $utilities.create_batch_exporter(exportPathArg)
    unless traversal_options.empty?
      log(""Setting traversal options: #{traversal_options}"", severity: :debug)
      exporter.set_traversal_options(traversal_options)
    end
    unless skipSlipsheetedItemsArg.nil?
      log(""Setting Skip Natives Slipsheeted Items: #{skipSlipsheetedItemsArg}"", severity: :debug)
      exporter.setSkipNativesSlipsheetedItems(skipSlipsheetedItemsArg)
    end
    unless numberingOptionsArg.nil?
      log(""Setting numbering options: #{numberingOptionsArg}"", severity: :debug)
      exporter.setNumberingOptions(numberingOptionsArg)
    end
    unless parallelProcessingSettingsArg.nil?
      log(""Setting parallel processing settings: #{parallelProcessingSettingsArg}"", severity: :debug)
      exporter.setParallelProcessingSettings(parallelProcessingSettingsArg)
    end
    unless exportOptionsArg.nil?
      exportOptionsArg.each do |product, options|
        log(""Adding #{product} to export with options: #{options}"", severity: :debug)
        exporter.add_product(product, options)
      end
    end
    if prod_profile.nil? and !loadFileTypeArg.eql?('none')
        log(""Adding load file to export: #{loadFileTypeArg}"", severity: :debug)
      if loadFileOptionsArg.nil?
        exporter.add_load_file(loadFileTypeArg)
      else
        log(""Load file options: #{loadFileOptionsArg}"", severity: :debug)
        exporter.add_load_file(loadFileTypeArg, loadFileOptionsArg)
      end
    end
    exporter.after_export do |details|
      log 'Export finished'
      log(""Details: #{details}'"", severity: :trace)
      log ""Total exported: #{details.get_items.size}""
      failed = details.get_failed_items
      log ""Total failed: #{failed.size}""
      if !failedItemsTagArg.nil? and failed.size > 0
        log ""Tagging #{failed.size} failed items with '#{failedItemsTagArg}'.""
        $utilities.get_bulk_annotater.add_tag(failedItemsTagArg, failed)
      end
    end
    log 'Starting export'
    exporter.export_items(production_set)
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
    /// An array of file types and associated options to produce.
    /// If this is not set, the ProductionSet must have a production profile.
    /// See Nuix API <code>BatchExporter.addProduct()</code>
    /// for more details on the available options.
    /// </summary>
    [StepProperty(3)]
    [RubyArgument("exportOptionsArg")]
    [DefaultValueExplanation("The ProductionSet profile is used")]
    [Example(
        "(native: (path: 'NATIVE' naming: 'document_id'), text: (path: 'TEXT' naming: 'document_id'))"
    )]
    public IStep<Core.Entity>? ExportOptions { get; set; }

    /// <summary>
    /// Set the numbering options for the export.
    /// This setting has no effect if the production set has numbering options defined.
    /// See Nuix API <code>NumberingConfigurable.setNumberingOptions()</code>
    /// for more details on the available options.
    /// </summary>
    [StepProperty(4)]
    [RubyArgument("numberingOptionsArg")]
    [DefaultValueExplanation("Document ID numbering, starting with DOC-000000001")]
    [Example("(createProductionSet: false prefix: 'ABC' documentId: (startAt: 1 minWidth: 4))")]
    public IStep<Core.Entity>? NumberingOptions { get; set; }

    /// <summary>
    /// The options to use for creating the load file.
    /// This parameter has no effect if the production set has a production profile.
    /// See Nuix API <code>BatchExporter.addLoadFile()</code>
    /// for more details on the available options.
    /// </summary>
    [StepProperty(5)]
    [RubyArgument("loadFileOptionsArg")]
    [DefaultValueExplanation("Default options are used")]
    [Example("(metadataProfile: 'Profile' loadFileEntryLimit: 5000)")]
    public IStep<Core.Entity>? LoadFileOptions { get; set; }

    /// <summary>
    /// The type of load file to export.
    /// This parameter has no effect if the production set has a production profile.
    /// </summary>
    [StepProperty]
    [RubyArgument("loadFileTypeArg")]
    [DefaultValueExplanation(nameof(Enums.LoadFileType.Concordance))]
    public IStep<LoadFileType> LoadFileType { get; set; } =
        new EnumConstant<LoadFileType>(Enums.LoadFileType.Concordance);

    /// <summary>
    /// Sets the parallel processing settings to use.
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("Default processing settings used")]
    [RubyArgument("parallelProcessingSettingsArg")]
    [Example("(workerCount: 2 workerMemory: 4096)")]
    public IStep<Core.Entity>? ParallelProcessingSettings { get; set; }

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

    /// <summary>
    /// 
    /// </summary>
    [StepProperty]
    [RubyArgument("failedItemsTagArg")]
    [DefaultValueExplanation("Failed items are not tagged")]
    public IStep<StringStream>? FailedItemsTag { get; set; }
}

}
