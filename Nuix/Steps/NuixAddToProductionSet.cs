using CSharpFunctionalExtensions;
using Reductech.Sequence.Connectors.Nuix.Enums;
using Reductech.Sequence.Connectors.Nuix.Steps.Helpers;
using Reductech.Sequence.Core.Internal.Errors;

namespace Reductech.Sequence.Connectors.Nuix.Steps
{

/// <summary>
/// Run a search query in Nuix and add all items found to a production set.
/// Will create a new production set if one with the given name does not already exist.
/// </summary>
public sealed class
    NuixAddToProductionSetStepFactory : RubySearchStepFactory<NuixAddToProductionSet, Unit>
{
    private NuixAddToProductionSetStepFactory() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static RubyScriptStepFactory<NuixAddToProductionSet, Unit> Instance { get; } =
        new NuixAddToProductionSetStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion => new(7, 2);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>() { NuixFeature.PRODUCTION_SET };

    /// <inheritdoc />
    public override IReadOnlyCollection<IRubyHelper> RequiredHelpers { get; }
        = new List<IRubyHelper>
        {
            NuixSearch.Instance, NuixExpandSearch.Instance, NuixSortItems.Instance
        };

    /// <inheritdoc />
    public override string FunctionName => "AddToProductionSet";

    /// <inheritdoc />
    public override string RubyFunctionText => @"

    items = search(searchArg, searchOptionsArg, sortArg)
    return unless items.length > 0

    production_set = $current_case.findProductionSetByName(productionSetNameArg)

    if production_set.nil?
      log ""Production set '#{productionSetNameArg}' not found. Creating.""
      options = descriptionArg.nil? ? {} : { :description => descriptionArg }
      log(""Production set options: #{options}"", severity: :trace)
      production_set = $current_case.newProductionSet(productionSetNameArg, options)

      if productionProfileNameArg != nil
        log(""Setting production profile: #{productionProfileNameArg}"", severity: :debug)
        production_set.setProductionProfile(productionProfileNameArg)
      elsif productionProfilePathArg != nil
        log(""Loading production profile from #{productionProfilePathArg}"", severity: :debug)
        profileBuilder = $utilities.getProductionProfileBuilder()
        profile = profileBuilder.load(productionProfilePathArg)
        if profile.nil?
          write_error(""Could not find processing profile: #{productionProfilePathArg}"", terminating: true)
        end
        production_set.setProductionProfileObject(profile)
      end

      unless numberingOptionsArg.nil?
        log(""Numbering options: #{numberingOptionsArg}"", severity: :debug)
        production_set.set_numbering_options(numberingOptionsArg)
      end

      unless imagingOptionsArg.nil?
        log(""Imaging settings: #{imagingOptionsArg}"", severity: :debug)
        production_set.set_imaging_options(imagingOptionsArg)
      end

      unless stampingOptionsArg.nil?
        log(""Stamping options: #{stampingOptionsArg}"", severity: :debug)
        production_set.set_stamping_options(stampingOptionsArg)
      end

      unless textOptionsArg.nil?
        log(""Text settings: #{textOptionsArg}"", severity: :debug)
        production_set.set_text_settings(textOptionsArg)
      end

      log ""Successfully created '#{productionSetNameArg}' production set.""
    else
      log ""Existing production set '#{productionSetNameArg}' found""
    end

    all_items = expand_search(items, searchTypeArg)
    all_items = sort_items(all_items, itemSortOrderArg) unless itemSortOrderArg.nil?

    log ""Adding #{all_items.length} items to production set '#{productionSetNameArg}'""
    production_set.addItems(all_items)
    log('Finished adding items', severity: :debug)
";
}

/// <summary>
/// Run a search query in Nuix and add all items found to a production set.
/// Will create a new production set if one with the given name does not already exist.
/// </summary>
[Alias("NuixCreateProductionSet")]
public sealed class NuixAddToProductionSet : RubySearchStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixAddToProductionSetStepFactory.Instance;

    /// <summary>
    /// The production set to add results to. Will be created if it doesn't already exist.
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("productionSetNameArg")]
    [Alias("ProductionSet")]
    public IStep<StringStream> ProductionSetName { get; set; } = null!;

    /// <summary>
    /// Description of the production set.
    /// </summary>
    [StepProperty(3)]
    [RubyArgument("descriptionArg")]
    [DefaultValueExplanation("No description")]
    [Alias("Description")]
    public IStep<StringStream>? ProductionSetDescription { get; set; }

    /// <summary>
    /// The name of the Production profile to use.
    /// Cannot be used at the same time as ProductionProfilePath.
    /// This option only works if the ProductionSet is created by this Step.
    /// </summary>
    [RequiredVersion(NuixVersionKey, "7.2")]
    [StepProperty]
    [Example("MyProcessingProfile")]
    [DefaultValueExplanation("No profile set")]
    [RubyArgument("productionProfileNameArg")]
    [Alias("Profile")]
    public IStep<StringStream>? ProductionProfileName { get; set; }

    /// <summary>
    /// The path to the Production profile to use.
    /// Cannot be used at the same time as ProductionProfileName.
    /// This option only works if the ProductionSet is created by this Step.
    /// </summary>
    [RequiredVersion(NuixVersionKey, "7.6")]
    [StepProperty]
    [Example("C:/Profiles/MyProcessingProfile.xml")]
    [DefaultValueExplanation("No profile set")]
    [RubyArgument("productionProfilePathArg")]
    [Alias("ProfilePath")]
    public IStep<StringStream>? ProductionProfilePath { get; set; }

    /// <summary>
    /// Sort items before adding them to the production set.
    /// </summary>
    [StepProperty]
    [DefaultValueExplanation("Unsorted or by relevance. See SortSearch.")]
    [RubyArgument("itemSortOrderArg")]
    [Alias("SortItemsBy")]
    public IStep<SCLEnum<ItemSortOrder>>? ItemSortOrder { get; set; }

    /// <summary>
    /// Sets the imaging options for a production set.
    /// This option only works if the ProductionSet is created by this Step.
    /// Cannot be used at the same time as using a profile.
    /// See Nuix API <code>ImagingConfigurable.setImagingOptions()</code>
    /// for more details on the available options.
    /// </summary>
    [StepProperty]
    [RubyArgument("imagingOptionsArg")]
    [DefaultValueExplanation("None")]
    [Example(
        "(imageExcelSpreadsheets: true slipSheetContainers: true tiffFormat: 'GREYSCALE_LZW')"
    )]
    public IStep<Core.Entity>? ImagingOptions { get; set; }

    /// <summary>
    /// Set the numbering options for the production set.
    /// This option only works if the ProductionSet is created by this Step.
    /// See Nuix API <code>NumberingConfigurable.setNumberingOptions()</code>
    /// for more details on the available options.
    /// </summary>
    [StepProperty]
    [RubyArgument("numberingOptionsArg")]
    [DefaultValueExplanation("Document ID numbering, starting with DOC-000000001")]
    [Example("(createProductionSet: false prefix: 'ABC' documentId: (startAt: 1 minWidth: 4))")]
    public IStep<Core.Entity>? NumberingOptions { get; set; }

    /// <summary>
    /// Sets the stamping options to use when exporting a production set.
    /// This option only works if the ProductionSet is created by this Step.
    /// Cannot be used at the same time as using a profile.
    /// See Nuix API <code>StampingConfigurable.setStampingOptions()</code>
    /// for more details on the available options.
    /// </summary>
    [StepProperty]
    [RubyArgument("stampingOptionsArg")]
    [DefaultValueExplanation("None")]
    [Example("(footerCentre: (type: 'document_number'))")]
    public IStep<Core.Entity>? StampingOptions { get; set; }

    /// <summary>
    /// Sets the text settings for a production set.
    /// This option only works if the ProductionSet is created by this Step.
    /// Cannot be used at the same time as using a profile.
    /// See Nuix API <code>ProductionSet.setTextSettings()</code>
    /// for more details on the available options.
    /// </summary>
    [StepProperty]
    [RubyArgument("textOptionsArg")]
    [DefaultValueExplanation("None")]
    [Example("(lineSeparator: '\\n' encoding: 'UTF-8')")]
    public IStep<Core.Entity>? TextOptions { get; set; }

    /// <inheritdoc />
    public override Result<Unit, IError> VerifyThis(StepFactoryStore stepFactoryStore)
    {
        if (ProductionProfileName != null && ProductionProfilePath != null)
            return new SingleError(
                new ErrorLocation(this),
                ErrorCode.ConflictingParameters,
                nameof(ProductionProfileName),
                nameof(ProductionProfilePath)
            );

        if ((ProductionProfileName != null || ProductionProfilePath != null) &&
            (ImagingOptions != null || StampingOptions != null || TextOptions != null))
        {
            return new SingleError(
                new ErrorLocation(this),
                ErrorCode.ConflictingParameters,
                $"({nameof(ProductionProfileName)} or {nameof(ProductionProfilePath)})",
                $"({nameof(ImagingOptions)}, {StampingOptions}, or {nameof(TextOptions)})"
            );
        }

        return base.VerifyThis(stepFactoryStore);
    }
}

}
