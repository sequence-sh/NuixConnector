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
/// Creates a new case and opens it
/// </summary>
public sealed class NuixCreateCaseStepFactory : RubyScriptStepFactory<NuixCreateCase, Unit>
{
    private NuixCreateCaseStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixCreateCase, Unit> Instance { get; } =
        new NuixCreateCaseStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(2, 16);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>() { NuixFeature.CASE_CREATION };

    /// <inheritdoc />
    public override string FunctionName => "CreateCase";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    log 'Creating Case'

    the_case = $utilities.case_factory.create(pathArg,
    :name => nameArg,
    :description => descriptionArg,
    :investigator => investigatorArg)
    log 'Case Created'
    $current_case = the_case";
}

/// <summary>
/// Creates a new case and opens it.
/// </summary>
public sealed class NuixCreateCase : RubyScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override CasePathParameter CasePathParameter => new CasePathParameter.ChangesOpenCase(
        new RubyFunctionParameter(PathArg, nameof(CasePath), false)
    );

    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixCreateCaseStepFactory.Instance;

    /// <summary>
    /// The pathArg argument name in Ruby.
    /// </summary>
    public const string PathArg = "pathArg";

    /// <summary>
    /// The path to the directory to create the case in.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [Example("C:/Cases/MyCase")]
    [RubyArgument(PathArg)]
    [Alias("Directory")]
    public IStep<StringStream> CasePath { get; set; } = null!;

    /// <summary>
    /// The name of the case to create.
    /// </summary>
    [Required]
    [StepProperty(2)]
    [RubyArgument("nameArg")]
    [Alias("Name")]
    public IStep<StringStream> CaseName { get; set; } = null!;

    /// <summary>
    /// Name of the investigator.
    /// </summary>
    [Required]
    [StepProperty(3)]
    [RubyArgument("investigatorArg")]
    public IStep<StringStream> Investigator { get; set; } = null!;

    /// <summary>
    /// Description of the case.
    /// </summary>
    [StepProperty(4)]
    [RubyArgument("descriptionArg")]
    [DefaultValueExplanation("No Description")]
    public IStep<StringStream>? Description { get; set; }
}

}
