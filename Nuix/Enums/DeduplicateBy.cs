using System.ComponentModel;

namespace Reductech.Sequence.Connectors.Nuix.Enums;

/// <summary>
/// Whether to deduplicate as a family or individual
/// </summary>
public enum DeduplicateBy
{
    /// <summary>
    /// Deduplication by individual treats each item as an individual and an attachment or embedded item has the same priority for deduplication as a loose file.
    /// </summary>
    [Description("INDIVIDUAL")]
    [Display(Name = "INDIVIDUAL")]
    Individual,

    /// <summary>
    /// Items can be treated as a family where only the top-level item of a family is deduplicated and the descendants are classified as original or duplicate with their family as a group. The top-level item does not have to be in the set for its descendants to classified this way.
    /// </summary>
    [Description("FAMILY")]
    [Display(Name = "FAMILY")]
    Family
}
