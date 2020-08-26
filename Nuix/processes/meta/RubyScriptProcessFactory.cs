using System;
using System.Collections.Generic;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// A process that runs a ruby script against NUIX
    /// </summary>
    public abstract class RubyScriptProcessFactory<TProcess, TOutput> : SimpleRunnableProcessFactory<TProcess, TOutput>, IRubyScriptProcessFactory
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
                    MinVersion = NuixVersionHelper.DefaultRequiredVersion > RequiredVersion ? NuixVersionHelper.DefaultRequiredVersion : RequiredVersion
                };

                foreach (var feature in RequiredFeatures)
                    yield return new Requirement
                    {
                        Name = RubyScriptProcessUnit.NuixProcessName + feature
                    };
            }
        }




        /// <inheritdoc />
        public abstract Version RequiredVersion { get; }

        /// <inheritdoc />
        public abstract IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }

    }
}