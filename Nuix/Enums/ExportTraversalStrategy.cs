using System.ComponentModel;

namespace Reductech.EDR.Connectors.Nuix.Enums;

/// <summary>
/// Traversal strategy that is used when exporting items.
/// </summary>
public enum ExportTraversalStrategy
{
    /// <summary>
    /// Export only the items specified.
    /// </summary>
    [Description("items")]
    [Display(Name = "items")]
    Items,

    /// <summary>
    /// Export items and their descendants.
    /// </summary>
    [Description("items_and_descendants")]
    [Display(Name = "items_and_descendants")]
    ItemsAndDescendants,

    /// <summary>
    /// Export the top-level items for the items specified.
    /// </summary>
    [Description("top_level_items")]
    [Display(Name = "top_level_items")]
    TopLevelItems,

    /// <summary>
    /// Export the top-level items for the items specified, and any descendants of the top-level items.
    /// </summary>
    [Description("top_level_items_and_descendants")]
    [Display(Name = "top_level_items_and_descendants")]
    TopLevelItemsAndDescendants
}
