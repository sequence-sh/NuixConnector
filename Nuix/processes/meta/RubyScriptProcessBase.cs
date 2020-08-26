using System;
using System.Collections.Generic;
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
        public abstract Result<string, IRunErrors> TryCompileScript(ProcessState processState);

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


        internal abstract IEnumerable<(string argumentName, IRunnableProcess? argumentValue, bool valueCanBeNull)> GetArgumentValues();

        /// <summary>
        /// The method parameters.
        /// </summary>
        protected IEnumerable<Result<RubyMethodParameter, IRunErrors>> TryGetMethodParameters(ProcessState processState)
        {
            foreach (var a in GetArgumentValues())
            {
                if(a.argumentValue == null)
                    yield return new RubyMethodParameter(a.argumentName, null, a.valueCanBeNull);
                else
                {
                    var r = a.argumentValue.Run<object>(processState);

                    if (r.IsFailure)
                        yield return r.ConvertFailure<RubyMethodParameter>();
                    yield return new RubyMethodParameter(a.argumentName, r.Value.ToString(), a.valueCanBeNull);
                }
            }
        }
    }
}