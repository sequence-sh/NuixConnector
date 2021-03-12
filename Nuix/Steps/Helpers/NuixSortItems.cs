using Reductech.EDR.Connectors.Nuix.Steps.Meta;

namespace Reductech.EDR.Connectors.Nuix.Steps.Helpers
{

/// <summary>
/// Expand a collection of items using one of the expansion methods
/// available in <see cref="Enums.SearchType"/>, e.g. descendants,
/// families, conversation threads.
/// </summary>
public class NuixSortItems : IRubyHelper
{
    /// <inheritdoc />
    public string FunctionName { get; } = "SortItems";

    /// <inheritdoc />
    public string FunctionText { get; } = @"
def sort_items(items, sort_type)
    iutil = $utilities.get_item_utility
    case sort_type
      when 'position'
        log 'Sorting items by Position'
        sorted_items = iutil.sortItemsByPosition(items)
      when 'top_level_item_date'
        log 'Sorting items by TopLevelItemDate'
        sorted_items = iutil.sortItemsByTopLevelItemDate(items)
      when 'top_level_item_date_descending'
        log 'Sorting items by TopLevelItemDateDescending'
        sorted_items = iutil.sortItemsByTopLevelItemDateDescending(items)
      when 'document_id'
        log('The DocumentId sort order is only available when reordering production sets.', severity: :warn)
        sorted_items = items
    end
    return sorted_items
end
";

    private NuixSortItems() { }

    /// <summary>
    /// An instance of the helper function
    /// </summary>
    public static IRubyHelper Instance => new NuixSortItems();
}

}
