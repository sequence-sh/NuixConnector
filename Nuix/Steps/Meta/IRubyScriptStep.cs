using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;

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

        /// <summary>
        /// Compiles the script for this step.
        /// </summary>
        /// <returns></returns>
        Task<Result<string, IError>> TryCompileScriptAsync(StateMonad stateMonad, CancellationToken cancellationToken);

        /// <summary>
        /// Tries to convert this step into a ruby block.
        /// This may be a typed ruby block.
        /// This will fail if the ruby block is dependent on non-nuix functions, or if it sets any variables.
        /// </summary>
        public Result<IRubyBlock> TryConvert();
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