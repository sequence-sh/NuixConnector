namespace Reductech.EDR.Connectors.Nuix.Steps
{

/// <summary>
/// Migrates a case to the latest version if necessary.
/// </summary>
public sealed class NuixOpenCaseStepFactory : RubyScriptStepFactory<NuixOpenCase, Unit>
{
    private NuixOpenCaseStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixOpenCase, Unit> Instance { get; } =
        new NuixOpenCaseStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(7, 2);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>();

    /// <inheritdoc />
    public override string FunctionName => "OpenCase";

    /// <inheritdoc />
    public override string RubyFunctionText => @"open_case(pathArg)"; // very simple
}

/// <summary>
/// Migrates a case to the latest version if necessary.
/// </summary>
public sealed class NuixOpenCase : RubyScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override CasePathParameter CasePathParameter => new CasePathParameter.ChangesOpenCase(
        new RubyFunctionParameter(PathArg, nameof(CasePath), false)
    );

    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixOpenCaseStepFactory.Instance;

    /// <summary>
    /// The pathArg argument name in Ruby.
    /// </summary>
    public const string PathArg = "pathArg";

    /// <summary>
    /// The path to the case.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [Example("C:/Cases/MyCase")]
    [RubyArgument(PathArg)]
    [Alias("Case")]
    public IStep<StringStream> CasePath { get; set; } = null!;
}

}
