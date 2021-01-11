using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{

/// <summary>
/// Generates print previews for items in a production set.
/// </summary>
public sealed class
    NuixGeneratePrintPreviewsStepFactory : RubyScriptStepFactory<NuixGeneratePrintPreviews, Unit>
{
    private NuixGeneratePrintPreviewsStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixGeneratePrintPreviews, Unit> Instance { get; } =
        new NuixGeneratePrintPreviewsStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(5, 2);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>() { NuixFeature.PRODUCTION_SET };

    /// <inheritdoc />
    public override string FunctionName => "GeneratePrintPreviews";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    productionSet = currentCase.findProductionSetByName(productionSetNameArg)

    if(productionSet == nil)
        log ""Production Set Not Found""
    else
        log ""Production Set Found""

        options = {}

        resultMap = productionSet.generatePrintPreviews(options)

        log ""Print previews generated""
    end";
}

/// <summary>
/// Generates print previews for items in a production set.
/// </summary>
public sealed class NuixGeneratePrintPreviews : RubyScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixGeneratePrintPreviewsStepFactory.Instance;

    /// <summary>
    /// The production set to generate print previews for.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [RubyArgument("productionSetNameArg", 1)]
    [Alias("ProductionSet")]
    public IStep<StringStream> ProductionSetName { get; set; } = null!;
}

}
