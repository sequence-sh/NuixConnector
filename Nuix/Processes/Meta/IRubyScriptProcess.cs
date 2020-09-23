using CSharpFunctionalExtensions;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Processes.Meta
{
    /// <summary>
    /// A ruby script process.
    /// </summary>
    public interface IRubyScriptProcess : ICompoundRunnableProcess
    {
        /// <summary>
        /// The name of the function to run.
        /// </summary>
        string FunctionName { get; }

        /// <summary>
        /// Compiles the script for this process.
        /// </summary>
        /// <returns></returns>
        Result<string, IRunErrors> TryCompileScript(ProcessState processState);

        /// <summary>
        /// Tries to convert this process into a ruby block.
        /// This may be a typed ruby block.
        /// This will fail if the ruby block is dependent on non-nuix functions, or if it sets any variables.
        /// </summary>
        public Result<IRubyBlock> TryConvert();
    }

    /// <summary>
    /// A ruby script process
    /// </summary>
    public interface IRubyScriptProcess<T> : IRubyScriptProcess
    {
        /// <summary>
        /// The ruby factory to use for this process.
        /// </summary>
        IRubyScriptProcessFactory<T> RubyScriptProcessFactory { get; }
    }
}