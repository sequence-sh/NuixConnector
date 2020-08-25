using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// The base of a ruby script process.
    /// </summary>
    public abstract class RubyScriptProcessBase<T> : CompoundRunnableProcess<T>, IRubyScriptProcess<T>
    {
        /// <inheritdoc />
        public abstract string CompileScript();

        /// <inheritdoc />
        public override Result<T, IRunErrors> Run(ProcessState processState) => RunAsync(processState).Result;

        /// <summary>
        /// Runs this process asynchronously.
        /// </summary>
        protected abstract Task<Result<T, IRunErrors>> RunAsync(ProcessState processState);

        /// <summary>
        /// The factory to use for this process.
        /// </summary>
        public abstract IRubyScriptProcessFactory RubyScriptProcessFactory { get; }

        /// <inheritdoc />
        public override IRunnableProcessFactory RunnableProcessFactory => RubyScriptProcessFactory;


        /// <summary>
        /// Checks if the current set of arguments is valid.
        /// </summary>
        /// <returns></returns>
        internal abstract string ScriptText { get; }

        internal abstract string MethodName { get; }

        /// <summary>
        /// Required version of nuix, if it was changed by the parameters.
        /// </summary>
        public virtual Version? RunTimeNuixVersion => null;


        internal abstract IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues();

        /// <summary>
        /// The method parameters.
        /// </summary>
        protected IReadOnlyCollection<RubyMethodParameter> MethodParameters =>
            GetArgumentValues()
                .Select(x => new RubyMethodParameter(x.argumentName, x.argumentValue, x.valueCanBeNull)).ToList();
    }
}