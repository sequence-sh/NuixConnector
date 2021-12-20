namespace Reductech.Sequence.Connectors.Nuix.Steps;

/// <summary>
/// Calculate the total audited size for a case.
/// For this to work, the case material needs to have been ingested
/// with the calculateAuditedSize=true processing setting.
/// Nuix uses 1000 base for kb/mb/gb, not 1024.
/// </summary>
public sealed class
    NuixGetAuditedSizeStepFactory : RubyScriptStepFactory<NuixGetAuditedSize, SCLDouble>
{
    private NuixGetAuditedSizeStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixGetAuditedSize, SCLDouble> Instance { get; } =
        new NuixGetAuditedSizeStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(4, 2);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
        = new List<NuixFeature> { NuixFeature.ANALYSIS };

    /// <inheritdoc />
    public override string FunctionName => "GetAuditedSize";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    items = $current_case.search_unsorted(searchArg)
    log ""Calculating audited size for #{items.length} items""
    size = 0
    items.each {|i| size += i.get_audited_size }
    log ""Done. Total size: #{size}""
    return size
";
}

/// <summary>
/// Calculate the total audited size for a case.
/// For this to work, the case material needs to have been ingested
/// with the calculateAuditedSize=true processing setting.
/// Nuix uses 1000 base for kb/mb/gb, not 1024.
/// </summary>
[Alias("NuixCalculateAuditedSize")]
public sealed class NuixGetAuditedSize : RubyCaseScriptStepBase<SCLDouble>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<SCLDouble> RubyScriptStepFactory =>
        NuixGetAuditedSizeStepFactory.Instance;

    /// <summary>
    /// The search term to use for calculating audited size.
    /// This should be left as the default.
    /// </summary>
    [StepProperty(1)]
    [RubyArgument("searchArg")]
    [DefaultValueExplanation("flag:audited")]
    [Alias("Search")]
    public IStep<StringStream> SearchTerm { get; set; } = new SCLConstant<StringStream>("flag:audited");
}
