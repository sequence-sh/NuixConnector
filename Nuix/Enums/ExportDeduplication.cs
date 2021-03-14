using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Reductech.EDR.Connectors.Nuix.Enums
{

/// <summary>
/// Method of deduplication to use for a top-level item export.
/// </summary>
public enum ExportDeduplication
{
    /// <summary>
    /// No deduplication.
    /// </summary>
    [Description("none")]
    [Display(Name = "none")]
    None,

    /// <summary>
    /// If an item has no MD5, it is not removed.
    /// If two items have the same MD5, the first item by position is kept.
    /// </summary>
    [Description("md5")]
    [Display(Name = "md5")]
    // ReSharper disable once InconsistentNaming
    MD5,

    /// <summary>
    /// Same as MD5, except items assigned to different custodians are not treated as duplicates.
    /// </summary>
    [Description("md5_per_custodian")]
    [Display(Name = "md5_per_custodian")]
    // ReSharper disable once InconsistentNaming
    MD5PerCustodian
}

}
