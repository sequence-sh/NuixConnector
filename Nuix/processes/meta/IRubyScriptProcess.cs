using CSharpFunctionalExtensions;
using Reductech.EDR.Processes;
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
        Result<string, IRunErrors> TryCompileScript(ProcessState processState);
    }

    /// <summary>
    /// A ruby script process
    /// </summary>
    public interface IRubyScriptProcess<T> : IRubyScriptProcess
    {

    }
}