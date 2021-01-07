using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{

/// <summary>
/// A ruby script step.
/// </summary>
public interface IRubyScriptStep : ICompoundStep
{
    /// <summary>
    /// The name of the function to run.
    /// </summary>
    string FunctionName { get; }
}

/// <summary>
/// A ruby script step
/// </summary>
public interface IRubyScriptStep<T> : IRubyScriptStep, ICompoundStep<T>
{
    /// <summary>
    /// The ruby factory to use for this step.
    /// </summary>
    IRubyScriptStepFactory<T> RubyScriptStepFactory { get; }
}

}
