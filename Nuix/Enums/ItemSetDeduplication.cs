using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Reductech.EDR.Connectors.Nuix.Enums
{

/// <summary>
/// The means of deduplicating items by key and prioritizing originals in a tie-break. 
/// </summary>
public enum ItemSetDeduplication
{
    /// <summary>
    /// MD5RankedCustodian if a custodian ranking is given, MD5 otherwise
    /// </summary>
    Default,

    /// <summary>
    /// MD5 results in items with the same MD5 hash being identical. Tie breaking is by highest path order.
    /// </summary>
    [Description("MD5")]
    [Display(Name = "MD5")]
    MD5,

    /// <summary>
    /// MD5 Per Custodian results in items with the same MD5 hash and custodian being identical. Tie breaking is by highest path order.
    /// </summary>
    [Description("MD5 Per Custodian")]
    [Display(Name = "MD5 Per Custodian")]
    MD5PerCustodian,

    /// <summary>
    /// MD5 Ranked Custodian results in items with MD5 hash being identical. Tie breaking is by the item with the highest ranked custodian or highest path order if custodian ranking is equal.
    /// </summary>
    [Description("MD5 Ranked Custodian")]
    [Display(Name = "MD5 Ranked Custodian")]
    MD5RankedCustodian,

    /// <summary>
    /// Scripted results in items being deduplicated based on an expression defined by the script and passed to ItemSet.addItems. It is an error to add items to this Item Set without supplying the expression.
    /// </summary>
    [Description("Scripted")]
    [Display(Name = "Scripted")]
    Scripted,

    /// <summary>
    /// None results in all items being added to the set without deduplication.
    /// </summary>
    [Description("None")]
    [Display(Name = "None")]
    None
}

}
