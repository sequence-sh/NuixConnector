using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// The process factory for a ruby script process.
    /// </summary>
    public interface IRubyScriptProcessFactory<T> : IRunnableProcessFactory
    {
        /// <summary>
        /// The ruby function to run.
        /// </summary>
        IRubyFunction<T> RubyFunction { get; }
    }
}