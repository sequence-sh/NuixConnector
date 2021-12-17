using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps.Helpers;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta;

/// <summary>
/// A ruby script step that searches for items. Contains all the
/// parameters required for searching.
/// </summary>
public abstract class RubySearchStepBase<T> : RubyCaseScriptStepBase<T> where T : ISCLObject
{
    /// <summary>
    /// The Nuix search query. For more details on the supported syntax,
    /// please see the Nuix Workstation Search Guide.
    /// </summary>
    [Required]
    [StepProperty(1)]
    [Example("*.txt")]
    [RubyArgument("searchArg")]
    [Alias("Search")]
    public virtual IStep<StringStream> SearchTerm { get; set; } = null!;

    /// <summary>
    /// Pass additional search options to nuix.
    /// For an unsorted search (default) the only available option is defaultFields.
    /// When using <code>SortSearch=true</code> the options are defaultFields, order, and limit.
    /// Please see the nuix API for <code>Case.search</code>
    /// and <code>Case.searchUnsorted</code> for more details.
    /// </summary>
    [StepProperty]
    [RubyArgument("searchOptionsArg")]
    [DefaultValueExplanation("No search options provided")]
    public virtual IStep<Entity>? SearchOptions { get; set; }

    /// <summary>
    /// By default the search is not sorted by relevance which increases performance.
    /// Set this to true to sort the search by relevance and enabling additional SearchOptions.
    /// </summary>
    [StepProperty]
    [RubyArgument("sortArg")]
    [DefaultValueExplanation("false")]
    public virtual IStep<SCLBool>? SortSearch { get; set; }

    /// <summary>
    /// Defines the type of search that is done. By default only the items responsive to
    /// the search terms are tagged, but the result set can be augmented using this parameter.
    /// </summary>
    [StepProperty]
    [RubyArgument("searchTypeArg")]
    [DefaultValueExplanation("ItemsOnly")]
    public virtual IStep<SCLEnum<SearchType>> SearchType { get; set; } =
        new SCLConstant<SCLEnum<SearchType>>(new SCLEnum<SearchType>(Enums.SearchType.ItemsOnly));
}

/// <summary>
/// A step that runs a ruby script that performs an item search
/// and requires the NuixSearch and NuixExpandSearch helpers.
/// </summary>
public abstract class RubySearchStepFactory<TStep, TOutput> : RubyScriptStepFactory<TStep, TOutput>
    where TStep : RubyCaseScriptStepBase<TOutput>, new()
    where TOutput : ISCLObject
{
    /// <summary>
    /// Any helper functions required for this Step to execute. By default the
    /// requirements are NuixSearch and NuixExpandSearch helpers.
    /// </summary>
    public override IReadOnlyCollection<IRubyHelper> RequiredHelpers { get; }
        = new List<IRubyHelper> { NuixSearch.Instance, NuixExpandSearch.Instance };
}
