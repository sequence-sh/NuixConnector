using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    /// <summary>
    /// The step factory for a ruby script step.
    /// </summary>
    public interface IRubyScriptStepFactory<T> : IStepFactory
    {
        /// <summary>
        /// The ruby function to run.
        /// </summary>
        IRubyFunction<T> RubyFunction { get; }
    }
}