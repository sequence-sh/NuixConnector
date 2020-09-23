using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Processes.Meta
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
            = new Lazy<IRubyFunction<TOutput>>(() => new RubyFunction<TOutput>(FunctionName, RubyFunctionText, true,
                RubyFunctionParameter.GetRubyFunctionParameters<TProcess>())
            {
                RequiredNuixVersion = RequiredNuixVersion,
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
        public abstract string FunctionName { get; }

        /// <summary>
        /// The text of the ruby function. Not Including the header.
        /// </summary>
        public abstract string RubyFunctionText { get; }


        /// <summary>
        /// The Required Nuix version
        /// </summary>
        public abstract Version RequiredNuixVersion { get; }

        /// <inheritdoc />
        public override IProcessNameBuilder ProcessNameBuilder => new ProcessNameBuilderFromTemplate(FunctionName);

        /// <summary>
        /// The Required Nuix Features.
        /// </summary>
        public abstract IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }

        /// <inheritdoc />
        public override Maybe<IProcessCombiner> ProcessCombiner { get; } = Maybe<IProcessCombiner>.From(NuixProcessCombiner.Instance);
    }
}