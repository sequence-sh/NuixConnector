using CSharpFunctionalExtensions;

namespace Reductech.EDR.Connectors.Nuix.Steps;

/// <summary>
/// Migrates a case to the latest version if necessary.
/// </summary>
public sealed class NuixMigrateCaseStepFactory : RubyScriptStepFactory<NuixMigrateCase, Unit>
{
    private NuixMigrateCaseStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixMigrateCase, Unit> Instance { get; } =
        new NuixMigrateCaseStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(7, 2);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>();

    /// <inheritdoc />
    public override string FunctionName => "MigrateCase";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    log ""Opening Case, migrating if necessary""
    close_case

    options = {migrate: true}

    the_case = $utilities.case_factory.open(pathArg, options)

    the_case.close
    log ""Case Closed""";
}

/// <summary>
/// Migrates a case to the latest version if necessary.
/// </summary>
public sealed class NuixMigrateCase : RubyScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixMigrateCaseStepFactory.Instance;

    /// <inheritdoc />
    public override CasePathParameter CasePathParameter =>
        new CasePathParameter.ChangesOpenCase(Maybe<RubyFunctionParameter>.None);

    /// <summary>
    /// The path to the case.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [Example("C:/Cases/MyCase")]
    [RubyArgument("pathArg")]
    [Alias("Case")]
    public IStep<StringStream> CasePath { get; set; } = null!;
}
