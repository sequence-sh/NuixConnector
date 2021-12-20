using System.ComponentModel;

namespace Reductech.Sequence.Connectors.Nuix.Enums;

/// <summary>
/// Selects the method of sorting a set of items
/// </summary>
public enum ItemSortOrder
{
    /// <summary>
    /// Default sort order (fastest). Sorts by position within the evidence tree.
    /// Sorts as documented in ItemUtility.sortItemsByPosition(Collection).
    /// </summary>
    [Description("position")]
    [Display(Name = "position")]
    Position,

    /// <summary>
    /// Sorts families by top-level item date and then sorts each individual family
    /// into position within the evidence tree.
    /// Sorts as documented in ItemUtility.sortItemsByTopLevelItemDate(Collection).
    /// </summary>
    [Description("top_level_item_date")]
    [Display(Name = "top_level_item_date")]
    TopLevelItemDate,

    /// <summary>
    /// Sorts families descending by top-level item date and then sorts each individual family
    /// into position within the evidence tree.
    /// Sorts as documented in ItemUtility.sortItemsByTopLevelItemDateDescending(Collection).
    /// </summary>
    [Description("top_level_item_date_descending")]
    [Display(Name = "top_level_item_date_descending")]
    TopLevelItemDateDescending,

    /// <summary>
    /// Sorts items based on their document IDs for the production set.
    /// Only valid when reordering production sets.
    /// </summary>
    [Description("document_id")]
    [Display(Name = "document_id")]
    DocumentId
}
