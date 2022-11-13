namespace Sequence.Connectors.Nuix.Steps;

/// <summary>
/// Excludes password protected items once they have been decrypted.
/// This only excludes encrypted original items that have a decrypted
/// child with the same name.
/// </summary>
public sealed class
    NuixExcludeDecryptedItemsStepFactory : RubyScriptStepFactory<NuixExcludeDecryptedItems, Unit>
{
    private NuixExcludeDecryptedItemsStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixExcludeDecryptedItems, Unit> Instance { get; } =
        new NuixExcludeDecryptedItemsStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(4, 2);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
        = new List<NuixFeature> { NuixFeature.ANALYSIS };

    /// <inheritdoc />
    public override string FunctionName => "ExcludeDecryptedItems";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    items = $current_case.search_unsorted(searchArg)
    log ""Encrypted items found: #{items.length}""
    return unless items.length > 0
    excluded = 0
    items.each do |item|
      children = item.get_children
      if children.count > 0
        if item.name == children[0].name
          excluded += 1
          item.exclude(exclusionArg)
          log(""Excluding #{item.name}"", severity: :trace)
        end
      end
    end
    if excluded > 0
      log ""Excluded #{excluded} items with reason '#{exclusionArg}'""
    else
      log ""No items found to exclude""
    end
";
}

/// <summary>
/// Excludes password protected items once they have been decrypted.
/// This only excludes encrypted original items that have a decrypted
/// child with the same name.
/// </summary>
public sealed class NuixExcludeDecryptedItems : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixExcludeDecryptedItemsStepFactory.Instance;

    /// <summary>
    /// The search term used to find decrypted items.
    /// </summary>
    [StepProperty(1)]
    [RubyArgument("searchArg")]
    [DefaultValueExplanation("has-exclusion:0 AND flag:encrypted")]
    [Alias("Search")]
    public IStep<StringStream> SearchTerm { get; set; } =
        new SCLConstant<StringStream>("has-exclusion:0 AND flag:encrypted");

    /// <summary>
    /// The exclusion reason.
    /// </summary>
    [StepProperty(2)]
    [RubyArgument("exclusionArg")]
    [DefaultValueExplanation("DecryptedItem")]
    [Alias("Exclusion")]
    public IStep<StringStream> ExclusionReason { get; set; } =
        new SCLConstant<StringStream>("DecryptedItem");
}
