using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// A process that runs a ruby script against NUIX
    /// </summary>
    public abstract class RubyScriptProcessFactory<TProcess, TOutput> : SimpleRunnableProcessFactory<TProcess, TOutput>, IRubyScriptProcessFactory<TOutput>
        where TProcess : IRubyScriptProcess<TOutput>, new()
    {
        /// <inheritdoc />
        public override IEnumerable<Requirement> Requirements
        {
            get
            {
                yield return new Requirement
                {
                    Name = RubyScriptProcessUnit.NuixProcessName,
                    MinVersion = NuixVersionHelper.DefaultRequiredVersion > RubyFunction.RequiredNuixVersion ? NuixVersionHelper.DefaultRequiredVersion : RubyFunction.RequiredNuixVersion
                };

                foreach (var feature in RubyFunction.RequiredNuixFeatures)
                    yield return new Requirement
                    {
                        Name = RubyScriptProcessUnit.NuixProcessName + feature
                    };
            }
        }

        /// <summary>
        /// Creates a new RubyScriptProcessFactory.
        /// </summary>
        protected RubyScriptProcessFactory()
        {
            _lazyRubyFunction
            = new Lazy<IRubyFunction<TOutput>>(() => new RubyFunction<TOutput>(MethodName, ScriptText, true,
                RubyFunctionParameter.GetRubyFunctionParameters<TProcess>())
            {
                RequiredNuixVersion = RequiredVersion,
                RequiredNuixFeatures = RequiredFeatures
            });
        }


        private readonly Lazy<IRubyFunction<TOutput>> _lazyRubyFunction;

        /// <summary>
        /// The ruby function to run.
        /// </summary>
        public IRubyFunction<TOutput> RubyFunction => _lazyRubyFunction.Value ;


        /// <summary>
        /// The Name of the Ruby Function.
        /// </summary>
        public abstract string MethodName { get; } //TODO rename FunctionName

        /// <summary>
        /// The text of the ruby function. Not Including the header.
        /// </summary>
        public abstract string ScriptText { get; } //TODO rename


        /// <summary>
        /// The Required Nuix version
        /// </summary>
        public abstract Version RequiredVersion { get; } //TODO rename RequiredNuixVersion

        /// <summary>
        /// The Required Nuix Features.
        /// </summary>
        public abstract IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }

        /// <inheritdoc />
        public override Maybe<IProcessCombiner> ProcessCombiner { get; } = Maybe<IProcessCombiner>.From(NuixProcessCombiner.Instance);
    }
}