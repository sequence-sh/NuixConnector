using System.ComponentModel;

namespace Reductech.EDR.Connectors.Nuix.Enums;

/// <summary>
/// Types of files that can be produced when exporting data from Nuix
/// </summary>
public enum ExportProduct
{
    /// <summary>
    /// Native file
    /// </summary>
    [Description("native")]
    [Display(Name = "native")]
    Native,

    /// <summary>
    /// Text file
    /// </summary>
    [Description("text")]
    [Display(Name = "text")]
    Text,

    /// <summary>
    /// PDF images of the natives
    /// </summary>
    [Description("pdf")]
    [Display(Name = "pdf")]
    // ReSharper disable once InconsistentNaming
    PDF,

    /// <summary>
    /// TIFF images of the natives
    /// </summary>
    [Description("tiff")]
    [Display(Name = "tiff")]
    // ReSharper disable once InconsistentNaming
    TIFF,

    /// <summary>
    /// XHTML metadata report
    /// </summary>
    [Description("xhtml_report")]
    [Display(Name = "xhtml_report")]
    XhtmlReport,

    /// <summary>
    /// Image thumbnails of the natives
    /// </summary>
    [Description("thumbnail")]
    [Display(Name = "thumbnail")]
    Thumbnail
}
