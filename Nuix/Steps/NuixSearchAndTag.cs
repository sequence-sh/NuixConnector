namespace Reductech.Sequence.Connectors.Nuix.Steps;

/// <summary>
/// Searches a NUIX case with a particular search string and tags all files it finds.
/// </summary>
public sealed class NuixSearchAndTagStepFactory : RubySearchStepFactory<NuixSearchAndTag, Unit>
{
    private NuixSearchAndTagStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixSearchAndTag, Unit> Instance { get; } =
        new NuixSearchAndTagStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(7, 0);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
        = new List<NuixFeature> { NuixFeature.ANALYSIS };

    /// <inheritdoc />
    public override string FunctionName => "SearchAndTag";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    items = search(searchArg, searchOptionsArg, sortArg)
    return unless items.length > 0

    all_items = expand_search(items, searchTypeArg)

    items_processed = 0
    $utilities.get_bulk_annotater.add_tag(tagArg, all_items) { items_processed += 1 }

    log ""Items tagged: #{items_processed}""
";
}

/// <summary>
/// Searches a NUIX case with a particular search string and tags all files it finds.
/// </summary>
public sealed class NuixSearchAndTag : RubySearchStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixSearchAndTagStepFactory.Instance;

    /// <summary>
    /// The tag to assign to found results.
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("tagArg")]
    public IStep<StringStream> Tag { get; set; } = null!;
}
