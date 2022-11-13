namespace Sequence.Connectors.Nuix.Steps;

/// <summary>
/// Run a search query in Nuix and exclude all items found.
/// </summary>
public sealed class
    NuixSearchAndExcludeStepFactory : RubySearchStepFactory<NuixSearchAndExclude, Unit>
{
    private NuixSearchAndExcludeStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixSearchAndExclude, Unit> Instance { get; } =
        new NuixSearchAndExcludeStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(7, 0);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
        = new List<NuixFeature> { NuixFeature.ANALYSIS };

    /// <inheritdoc />
    public override string FunctionName => "SearchAndExclude";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    items = search(searchArg, searchOptionsArg, sortArg)
    return unless items.length > 0

    all_items = expand_search(items, searchTypeArg)
    
    unless tagArg.nil?
      items_tagged = 0
      $utilities.get_bulk_annotater.add_tag(tagArg, all_items) { items_tagged += 1 }
      log ""Items tagged: #{items_tagged}""
    end

    exclude_reason = exclusionArg.nil? ? searchArg : exclusionArg
    items_excluded = 0
    $utilities.get_bulk_annotater.exclude(exclude_reason, all_items) { items_excluded += 1 }
    log ""Items excluded: #{items_excluded}""
";
}

/// <summary>
/// Run a search query in Nuix and exclude all items found.
/// </summary>
public sealed class NuixSearchAndExclude : RubySearchStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixSearchAndExcludeStepFactory.Instance;

    /// <summary>
    /// The exclusion reason
    /// </summary>
    [StepProperty(2)]
    [RubyArgument("exclusionArg")]
    [DefaultValueExplanation("The SearchTerm is used as the exclusion reason")]
    [Alias("Exclusion")]
    public IStep<StringStream>? ExclusionReason { get; set; }

    /// <summary>
    /// The tag to assign to the excluded items
    /// </summary>
    [StepProperty(3)]
    [RubyArgument("tagArg")]
    [DefaultValueExplanation("Items will not be tagged")]
    public IStep<StringStream>? Tag { get; set; }
}
