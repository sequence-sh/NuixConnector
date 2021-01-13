using CSharpFunctionalExtensions;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{

/// <summary>
/// The casePath parameter
/// </summary>
public record CasePathParameter
{
    private CasePathParameter() { }

    /// <summary>
    /// This function doesn't need an opened case
    /// </summary>
    public record NoCasePath : CasePathParameter;

    /// <summary>
    /// This function opens a case
    /// </summary>
    public record OpensCase(RubyFunctionParameter Parameter) : CasePathParameter;

    /// <summary>
    /// This function uses a case
    /// </summary>
    public record UsesCase(RubyFunctionParameter Parameter) : CasePathParameter;
}

}
