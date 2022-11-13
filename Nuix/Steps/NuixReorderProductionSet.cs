using Sequence.Connectors.Nuix.Enums;

namespace Sequence.Connectors.Nuix.Steps;

/// <summary>
/// Reorders and renumbers the items in a production set.
/// </summary>
public sealed class
    NuixReorderProductionSetStepFactory : RubyScriptStepFactory<NuixReorderProductionSet, Unit>
{
    private NuixReorderProductionSetStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixReorderProductionSet, Unit> Instance { get; } =
        new NuixReorderProductionSetStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(5, 2);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>() { NuixFeature.PRODUCTION_SET };

    /// <inheritdoc />
    public override string FunctionName => "RenumberProductionSet";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    production_set = $current_case.find_production_set_by_name(productionSetNameArg)

    if production_set.nil?
      log(""Production set '#{productionSetNameArg}' not found."", severity: :warn)
    else
      log ""Renumbering production set '#{productionSetNameArg}' using '#{sortOrderArg.gsub('_',' ')}'""
      production_set.renumber({ 'sortOrder' => sortOrderArg })
      log('Renumbering finished', severity: :debug)
    end
";
}

/// <summary>
/// Reorders and renumbers the items in a production set.
/// </summary>
[Alias("NuixReorderProduction")]
public sealed class NuixReorderProductionSet : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixReorderProductionSetStepFactory.Instance;

    /// <summary>
    /// The production set to reorder.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [RubyArgument("productionSetNameArg")]
    [Alias("Set")]
    public IStep<StringStream> ProductionSetName { get; set; } = null!;

    /// <summary>
    /// The method of sorting items during the renumbering.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation(nameof(ItemSortOrder.Position))]
    [RubyArgument("sortOrderArg")]
    [Alias("Order")]
    [Alias("ItemSort")]
    public IStep<SCLEnum<ItemSortOrder>> SortOrder { get; set; } =
        new SCLConstant<SCLEnum<ItemSortOrder>>(new SCLEnum<ItemSortOrder>(ItemSortOrder.Position));
}
