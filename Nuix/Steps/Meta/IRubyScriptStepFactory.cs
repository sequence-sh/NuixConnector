using System;
using System.Collections.Generic;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{

/// <summary>
/// The step factory for a ruby script step.
/// </summary>
public interface IRubyScriptStepFactory<T> : IStepFactory
{
    /// <summary>
    /// The ruby function to run.
    /// </summary>
    RubyFunction<T> RubyFunction { get; }

    /// <summary>
    /// The Required Nuix version
    /// </summary>
    Version RequiredNuixVersion { get; }

    /// <summary>
    /// The Required Nuix Features.
    /// </summary>
    IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
}

}
