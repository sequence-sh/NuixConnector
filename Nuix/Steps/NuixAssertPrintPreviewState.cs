using Reductech.EDR.Connectors.Nuix.Enums;

namespace Reductech.EDR.Connectors.Nuix.Steps
{

/// <summary>
/// Checks the print preview state of the production set.
/// </summary>
public sealed class
    NuixAssertPrintPreviewStateStepFactory : RubyScriptStepFactory<NuixAssertPrintPreviewState, Unit
    >
{
    private NuixAssertPrintPreviewStateStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixAssertPrintPreviewState, Unit> Instance { get; } =
        new NuixAssertPrintPreviewStateStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(5, 2);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>() { NuixFeature.PRODUCTION_SET, NuixFeature.ANALYSIS };

    /// <inheritdoc />
    public override string FunctionName => "GetPrintPreviewState";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    productionSet = $current_case.findProductionSetByName(productionSetNameArg)

    if(productionSet == nil)
        raise 'Production Set Not Found'
        
        exit
    else
        r = productionSet.getPrintPreviewState()

        if r.to_s.downcase == expectedStateArg.to_s.downcase
            log ""Print preview state was #{r}, as expected.""
        else
            raise ""Print preview state was #{r}, but expected #{expectedStateArg}""
            exit
        end
    end";
}

/// <summary>
/// Checks the print preview state of the production set.
/// </summary>
public sealed class NuixAssertPrintPreviewState : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixAssertPrintPreviewStateStepFactory.Instance;

    /// <summary>
    /// The production set to reorder.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [RubyArgument("productionSetNameArg")]
    [Alias("ProductionSet")]
    public IStep<StringStream> ProductionSetName { get; set; } = null!;

    /// <summary>
    /// The expected print preview state of the production set;
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation(nameof(PrintPreviewState.All))]
    [RubyArgument("expectedStateArg")]
    [Alias("HasState")]
    public IStep<PrintPreviewState> ExpectedState { get; set; } =
        new EnumConstant<PrintPreviewState>(PrintPreviewState.All);
}

}
