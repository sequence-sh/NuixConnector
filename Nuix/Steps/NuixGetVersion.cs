namespace Sequence.Connectors.Nuix.Steps;

/// <summary>
/// Returns the Nuix version
/// </summary>
public sealed class NuixGetVersionFactory : RubyScriptStepFactory<NuixGetVersion, StringStream>
{
    private NuixGetVersionFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixGetVersion, StringStream> Instance { get; } =
        new NuixGetVersionFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(7, 0);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>();

    /// <inheritdoc />
    public override string FunctionName => "GetVersion";

    /// <inheritdoc />
    public override string RubyFunctionText => @"return NUIX_VERSION";
}

/// <summary>
/// Returns the Nuix version
/// </summary>
public sealed class NuixGetVersion : RubyCaseScriptStepBase<StringStream>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<StringStream> RubyScriptStepFactory =>
        NuixGetVersionFactory.Instance;
}
