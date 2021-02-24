using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Reductech.EDR.Connectors.Nuix.Enums
{

/// <summary>
/// Selects the method of sorting items during production set sort ordering
/// </summary>
public enum ProductionSetSortOrder
{
    /// <summary>
    /// Default sort order (fastest). Sorts as documented in ItemSorter.sortItemsByPosition(List).
    /// </summary>
    [Description("position")]
    [Display(Name = "position")]
    Position,

    /// <summary>
    /// Sorts as documented in ItemSorter.sortItemsByTopLevelItemDate(List).
    /// </summary>
    [Description("top_level_item_date")]
    [Display(Name = "top_level_item_date")]
    TopLevelItemDate,

    /// <summary>
    /// Sorts as documented in ItemSorter.sortItemsByTopLevelItemDateDescending(List).
    /// </summary>
    [Description("top_level_item_date_descending")]
    [Display(Name = "top_level_item_date_descending")]
    TopLevelItemDateDescending,

    /// <summary>
    /// Sorts items based on their document IDs for the production set.
    /// </summary>
    [Description("document_id")]
    [Display(Name = "document_id")]
    DocumentId
}

}
