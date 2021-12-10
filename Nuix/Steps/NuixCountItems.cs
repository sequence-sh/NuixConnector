namespace Reductech.EDR.Connectors.Nuix.Steps;

/// <summary>
/// Returns the number of items matching a particular search term
/// </summary>
public sealed class NuixCountItemsStepFactory : RubyScriptStepFactory<NuixCountItems, int>
{
    private NuixCountItemsStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixCountItems, int> Instance { get; } =
        new NuixCountItemsStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(7, 0);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>();

    /// <inheritdoc />
    public override string FunctionName => "CountItems";

    /// <inheritdoc />
    public override string RubyFunctionText => @"

    log ""Searching for items: #{searchArg}""

    searchOptions = searchOptionsArg.nil? ? {} : searchOptionsArg
    log(""Search options: #{searchOptions}"", severity: :trace)

    count = $current_case.count(searchArg, searchOptions)

    log ""Items found: #{count}""

    return count
";
}

/// <summary>
/// Returns the number of items matching a particular search term
/// </summary>
public sealed class NuixCountItems : RubyCaseScriptStepBase<int>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<int> RubyScriptStepFactory =>
        NuixCountItemsStepFactory.Instance;

    /// <summary>
    /// The search term to count.
    /// </summary>
    [Required]
    [Example("*.txt")]
    [StepProperty(1)]
    [RubyArgument("searchArg")]
    [Alias("Search")]
    public IStep<StringStream> SearchTerm { get; set; } = null!;

    /// <summary>
    /// Pass additional search options to nuix. Options available:
    ///   - defaultFields: field(s) to query against when not present in the search string.
    /// Please see the nuix API for <code>Case.searchUnsorted</code> for more details.
    /// </summary>
    [StepProperty(2)]
    [RubyArgument("searchOptionsArg")]
    [DefaultValueExplanation("No search options provided")]
    public IStep<Entity>? SearchOptions { get; set; }
}
