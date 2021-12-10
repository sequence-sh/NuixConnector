namespace Reductech.EDR.Connectors.Nuix.Steps.Meta;

/// <summary>
/// A helper function that can be executed in other ruby scripts.
/// </summary>
public interface IRubyHelper
{
    /// <summary>
    /// The name of this function. Should be unique.
    /// </summary>
    string FunctionName { get; }

    /// <summary>
    /// The function definition.
    /// </summary>
    string FunctionText { get; }
}