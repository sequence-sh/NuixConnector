using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Reductech.EDR.Connectors.Nuix.Enums
{

/// <summary>
/// Defines the type of search that is performed.
/// </summary>
public enum SearchType
{
    /// <summary>
    /// Return only the items responsive to the search terms.
    /// </summary>
    [Description("items")]
    [Display(Name = "items")]
    ItemsOnly,

    /// <summary>
    /// Return only the descendants of the items responsive to the search terms.
    /// </summary>
    [Description("descendants")]
    [Display(Name = "descendants")]
    Descendants,

    /// <summary>
    /// Find entire families.
    /// </summary>
    [Description("families")]
    [Display(Name = "families")]
    Families,

    /// <summary>
    /// Same as Descendants, but including the items responsive to the search terms.
    /// </summary>
    [Description("items_descendants")]
    [Display(Name = "items_descendants")]
    ItemsAndDescendants,

    /// <summary>
    /// Finds items and their duplicates.
    /// </summary>
    [Description("items_duplicates")]
    [Display(Name = "items_duplicates")]
    ItemsAndDuplicates,

    /// <summary>
    /// Items responsive to the search terms and all items in the same discussion threads.
    /// </summary>
    [Description("thread_items")]
    [Display(Name = "thread_items")]
    ThreadItems,

    /// <summary>
    /// Returns the top-level items for the collection returned by a search.
    /// </summary>
    [Description("toplevel_items")]
    [Display(Name = "toplevel_items")]
    TopLevelItems
}

}
