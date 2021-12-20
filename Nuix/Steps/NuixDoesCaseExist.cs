using CSharpFunctionalExtensions;

namespace Reductech.Sequence.Connectors.Nuix.Steps;

/// <summary>
/// Returns whether or not a case exists.
/// </summary>
public sealed class NuixDoesCaseExistStepFactory : RubyScriptStepFactory<NuixDoesCaseExist, SCLBool>
{
    private NuixDoesCaseExistStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixDoesCaseExist, SCLBool> Instance { get; } =
        new NuixDoesCaseExistStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(2, 16);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>();

    /// <inheritdoc />
    public override string FunctionName => "DoesCaseExist";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    begin
        close_case()
        log ""Trying to open case""
        the_case = $utilities.case_factory.open(pathArg)
        the_case.close()
        return true
    rescue => ex
        log(""Case does not exist: #{ex}"")
        return false
    end
";
}

/// <summary>
/// Returns whether or not a case exists.
/// </summary>
public sealed class NuixDoesCaseExist : RubyScriptStepBase<SCLBool>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<SCLBool> RubyScriptStepFactory =>
        NuixDoesCaseExistStepFactory.Instance;

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
