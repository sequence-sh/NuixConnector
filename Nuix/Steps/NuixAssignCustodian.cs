namespace Reductech.Sequence.Connectors.Nuix.Steps;

/// <summary>
/// Run a search query on a nuix case and assign found items to a custodian.
/// </summary>
public sealed class NuixAssignCustodianFactory : RubySearchStepFactory<NuixAssignCustodian, Unit>
{
    private NuixAssignCustodianFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixAssignCustodian, Unit> Instance { get; } =
        new NuixAssignCustodianFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(5, 0);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
        = new List<NuixFeature> { NuixFeature.ANALYSIS };

    /// <inheritdoc />
    public override string FunctionName => "AssignCustodian";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    items = search(searchArg, searchOptionsArg, sortArg)
    return unless items.length > 0

    all_items = expand_search(items, searchTypeArg)

    items_processed = 0
    $utilities.get_bulk_annotater.assign_custodian(custodianArg, all_items) {|item| items_processed += 1 }

    log ""#{items_processed} items assigned to custodian #{custodianArg}""
";
}

/// <summary>
/// Run a search query on a nuix case and assign found items to a custodian.
/// </summary>
public sealed class NuixAssignCustodian : RubySearchStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixAssignCustodianFactory.Instance;

    /// <summary>
    /// The custodian to assign.
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("custodianArg")]
    public IStep<StringStream> Custodian { get; set; } = null!;
}
