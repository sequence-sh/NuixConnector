namespace Sequence.Connectors.Nuix.Steps;

/// <summary>
/// Annotates a document ID list to add production set names to it.
/// </summary>
public class
    NuixAnnotateDocumentIdListStepFactory : RubyScriptStepFactory<NuixAnnotateDocumentIdList, Unit>
{
    private NuixAnnotateDocumentIdListStepFactory() { }

    /// <summary>
    /// The instance
    /// </summary>
    public static RubyScriptStepFactory<NuixAnnotateDocumentIdList, Unit> Instance { get; } =
        new NuixAnnotateDocumentIdListStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(7, 4);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>() { NuixFeature.PRODUCTION_SET };

    /// <inheritdoc />
    public override string FunctionName => "AnnotateDocumentIds";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    productionSet = $current_case.findProductionSetByName(productionSetNameArg)

    if(productionSet == nil)
        log ""Production Set Not Found""
    else
        log ""Production Set Found""

        options =
        {
            dataPath: dataPathArg
        }
        resultMap = productionSet.annotateDocumentIdList(options)
        log resultMap
    end";
}

/// <summary>
/// Annotates a document ID list to add production set names to it.
/// </summary>
public class NuixAnnotateDocumentIdList : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixAnnotateDocumentIdListStepFactory.Instance;

    /// <summary>
    /// The production set to get names from.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [RubyArgument("productionSetNameArg")]
    [Alias("ProductionSet")]
    public IStep<StringStream> ProductionSetName { get; set; } = null!;

    /// <summary>
    /// Specifies the file path of the document ID list.
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("dataPathArg")]
    [Alias("IdList")]
    public IStep<StringStream> DataPath { get; set; } = null!;
}
