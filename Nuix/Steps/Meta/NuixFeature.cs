using System.Diagnostics.CodeAnalysis;

namespace Reductech.Sequence.Connectors.Nuix.Steps.Meta;

/// <summary>
/// A Nuix feature requirement
/// </summary>
[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum NuixFeature
{
    #pragma warning disable 1591
    ANALYSIS,

    CASE_CREATION,

    EXPORT_ITEMS,

    METADATA_IMPORT,

    OCR_PROCESSING,

    PRODUCTION_SET
    #pragma warning restore 1591
}