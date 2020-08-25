using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// A ruby script process.
    /// </summary>
    public interface IRubyScriptProcess : ICompoundRunnableProcess
    {
        /// <summary>
        /// Compiles the script for this process.
        /// </summary>
        /// <returns></returns>
        public string CompileScript();
    }

    /// <summary>
    /// A ruby script process
    /// </summary>
    public interface IRubyScriptProcess<T> : IRubyScriptProcess
    {

    }
}