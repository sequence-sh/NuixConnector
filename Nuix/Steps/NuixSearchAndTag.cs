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
/// Searches a NUIX case with a particular search string and tags all files it finds.
/// </summary>
public sealed class NuixSearchAndTagStepFactory : RubyScriptStepFactory<NuixSearchAndTag, Unit>
{
    private NuixSearchAndTagStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixSearchAndTag, Unit> Instance { get; } =
        new NuixSearchAndTagStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(2, 16);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
        = new List<NuixFeature> { NuixFeature.ANALYSIS };

    /// <inheritdoc />
    public override string FunctionName => "SearchAndTag";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    the_case = $utilities.case_factory.open(pathArg)
    log ""Searching for '#{searchArg}'""

    searchOptions = {}
    items = the_case.search(searchArg, searchOptions)
    log ""#{items.length} found""

    j = 0

    items.each {|i|
       added = i.addTag(tagArg)
       j += 1 if added
    }

    log ""#{j} items tagged with #{tagArg}""
    the_case.close";
}

/// <summary>
/// Searches a NUIX case with a particular search string and tags all files it finds.
/// </summary>
public sealed class NuixSearchAndTag : RubyScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixSearchAndTagStepFactory.Instance;

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
    /// The term to search for.
    /// </summary>
    [Required]
    [StepProperty(2)]
    [Example("*.txt")]
    [RubyArgument("searchArg", 2)]
    [Alias("Search")]
    public IStep<StringStream> SearchTerm { get; set; } = null!;

    /// <summary>
    /// The tag to assign to found results.
    /// </summary>
    [Required]
    [StepProperty(3)]
    [RubyArgument("tagArg", 3)]
    public IStep<StringStream> Tag { get; set; } = null!;
}

}
