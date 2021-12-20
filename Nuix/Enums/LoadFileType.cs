using System.ComponentModel;

namespace Reductech.Sequence.Connectors.Nuix.Enums;

/// <summary>
/// The type of load file to create when running an export.
/// </summary>
public enum LoadFileType
{
    /// <summary>
    /// No load file is created.
    /// </summary>
    [Description("none")]
    [Display(Name = "none")]
    None,

    /// <summary>
    /// Discover
    /// </summary>
    [Description("discover")]
    [Display(Name = "discover")]
    Discover,

    /// <summary>
    /// Ringtail (MDB)
    /// </summary>
    [Description("ringtail")]
    [Display(Name = "ringtail")]
    Ringtail,

    /// <summary>
    /// Concordance
    /// </summary>
    [Description("concordance")]
    [Display(Name = "concordance")]
    Concordance,

    /// <summary>
    /// Summation
    /// </summary>
    [Description("summation")]
    [Display(Name = "summation")]
    Summation,

    /// <summary>
    /// Discovery Radar
    /// </summary>
    [Description("discovery_radar")]
    [Display(Name = "discovery_radar")]
    DiscoveryRadar,

    /// <summary>
    /// DocuMatrix
    /// </summary>
    [Description("documatrix")]
    [Display(Name = "documatrix")]
    DocuMatrix,

    /// <summary>
    /// EDRM XML
    /// </summary>
    [Description("edrm_xml")]
    [Display(Name = "edrm_xml")]
    // ReSharper disable once InconsistentNaming
    EDRMXML,

    /// <summary>
    /// EDRM v1.2 XML ZIP
    /// </summary>
    [Description("edrm_xml_zip")]
    [Display(Name = "edrm_xml_zip")]
    // ReSharper disable once InconsistentNaming
    EDRMXMLZIP,

    /// <summary>
    /// IPRO
    /// </summary>
    [Description("ipro")]
    [Display(Name = "ipro")]
    // ReSharper disable once InconsistentNaming
    IPRO,

    /// <summary>
    /// XHTML summary report
    /// </summary>
    [Description("xhtml_summary_report")]
    [Display(Name = "xhtml_summary_report")]
    // ReSharper disable once InconsistentNaming
    XHTMLSummary,

    /// <summary>
    /// CSV summary report
    /// </summary>
    [Description("csv_summary_report")]
    [Display(Name = "csv_summary_report")]
    // ReSharper disable once InconsistentNaming
    CSVSummary,

    /// <summary>
    /// MD5 digest report
    /// </summary>
    [Description("md5_digest")]
    [Display(Name = "md5_digest")]
    // ReSharper disable once InconsistentNaming
    MD5Digest
}
