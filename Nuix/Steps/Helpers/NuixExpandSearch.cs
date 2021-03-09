using Reductech.EDR.Connectors.Nuix.Steps.Meta;

namespace Reductech.EDR.Connectors.Nuix.Steps.Helpers
{

/// <summary>
/// Expand a collection of items using one of the expansion methods
/// available in <see cref="Enums.SearchType"/>, e.g. descendants,
/// families, conversation threads.
/// </summary>
public class NuixExpandSearch : IRubyHelper
{
    /// <inheritdoc />
    public string FunctionName { get; } = "ExpandSearch";

    /// <inheritdoc />
    public string FunctionText { get; } = @"
def expand_search(items, search_type)
    if search_type.eql? 'items'
      all_items = items
    else
      iutil = $utilities.get_item_utility
      case search_type
        when 'descendants'
          all_items = iutil.find_descendants(items)
          log ""Descendants found: #{all_items.count}""
        when 'families'
          all_items = iutil.find_families(items)
          log ""Family items found: #{all_items.count}""
        when 'items_descendants'
          all_items = iutil.find_items_and_descendants(items)
          log ""Items and descendants found: #{all_items.count}""
        when 'items_duplicates'
          all_items = iutil.find_items_and_duplicates(items)
          log ""Items and duplicates found: #{all_items.count}""
        when 'thread_items'
          all_items = iutil.find_thread_items(items)
          log ""Thread items found: #{all_items.count}""
        when 'toplevel_items'
          all_items = iutil.find_top_level_items(items)
          log ""Top-level items found: #{all_items.count}""
      end
    end
    return all_items
end
";

    private NuixExpandSearch() { }

    /// <summary>
    /// An instance of the helper function
    /// </summary>
    public static IRubyHelper Instance => new NuixExpandSearch();
}

}
