using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{

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
    the_case = $utilities.case_factory.open(pathArg)
    productionSet = the_case.findProductionSetByName(productionSetNameArg)

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
    end

    the_case.close";
}

/// <summary>
/// Annotates a document ID list to add production set names to it.
/// </summary>
public class NuixAnnotateDocumentIdList : RubyScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixAnnotateDocumentIdListStepFactory.Instance;

    /// <summary>
    /// The path to the case.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [Example("C:/Cases/MyCase")]
    [RubyArgument("pathArg", 1)]
    [Alias("Case")]
    public IStep<StringStream> CasePath { get; set; } = null!;

    /// <summary>
    /// The production set to get names from.
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("productionSetNameArg", 2)]
    [Alias("ProductionSet")]
    public IStep<StringStream> ProductionSetName { get; set; } = null!;

    /// <summary>
    /// Specifies the file path of the document ID list.
    /// </summary>
    [Required]
    [StepProperty(3)]
    [RubyArgument("dataPathArg", 3)]
    [Alias("IdList")]
    public IStep<StringStream> DataPath { get; set; } = null!;
}

}
