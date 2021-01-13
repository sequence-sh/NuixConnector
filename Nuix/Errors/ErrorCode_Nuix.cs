using System.Diagnostics;
using JetBrains.Annotations;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Connectors.Nuix.Errors
{

/// <summary>
/// Error Code for Nuix
/// </summary>
public sealed record ErrorCode_Nuix : ErrorCodeBase
{
    /// <inheritdoc />
    private ErrorCode_Nuix([NotNull] string code) : base(code) { }

    /// <summary>
    /// A nuix function cannot have more than one Entity Array parameter.
    /// </summary>
    public static readonly ErrorCode_Nuix TooManyEntityStreams = new(nameof(TooManyEntityStreams));

    /// <summary>
    /// No case was open. Use NuixOpenCase to open a case.
    /// </summary>
    public static readonly ErrorCode_Nuix NoCaseOpen = new(nameof(NoCaseOpen));

    /// <inheritdoc />
    public override string GetFormatString()
    {
        var s = ErrorMessages_EN.ResourceManager.GetString(Code);

        Debug.Assert(s != null, nameof(s) + " != null");
        return s;
    }
}

}
