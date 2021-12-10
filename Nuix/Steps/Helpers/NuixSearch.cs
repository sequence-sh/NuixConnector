namespace Reductech.EDR.Connectors.Nuix.Steps.Helpers;

/// <summary>
/// Search a case and return a collection of items.
/// </summary>
public class NuixSearch : IRubyHelper
{
    /// <inheritdoc />
    public string FunctionName { get; } = "Search";

    /// <inheritdoc />
    public string FunctionText { get; } = @"
def search(search_term, search_options, sort)
  log ""Searching for '#{search_term}'""

  opts = search_options.nil? ? {} : search_options
  log(""Search options: #{opts}"", severity: :trace)

  if sort.nil? || !sort
    log('Search results will be unsorted', severity: :trace)
    items = $current_case.search_unsorted(search_term, opts)
  else
    log('Search results will be sorted', severity: :trace)
    items = $current_case.search(search_term, opts)
  end

  log ""Items found: #{items.length}""
  return items
end
";

    /// <summary>
    /// An instance of the helper function
    /// </summary>
    public static IRubyHelper Instance => new NuixSearch();
}
