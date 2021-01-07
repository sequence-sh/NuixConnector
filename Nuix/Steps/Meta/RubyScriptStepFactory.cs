using System;
using System.Collections.Generic;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{

/// <summary>
/// A step that runs a ruby script against NUIX
/// </summary>
public abstract class RubyScriptStepFactory<TStep, TOutput> : SimpleStepFactory<TStep, TOutput>,
                                                              IRubyScriptStepFactory<TOutput>
    where TStep : IRubyScriptStep<TOutput>, new()
{
    /// <inheritdoc />
    public override IEnumerable<Requirement> Requirements
    {
        get
        {
            yield return new Requirement
            {
                Name = RubyScriptStepBase<TStep>.NuixRequirementName,
                MinVersion =
                    NuixVersionHelper.DefaultRequiredVersion > RubyFunction.RequiredNuixVersion
                        ? NuixVersionHelper.DefaultRequiredVersion
                        : RubyFunction.RequiredNuixVersion
            };

            foreach (var feature in RubyFunction.RequiredNuixFeatures)
                yield return new Requirement
                {
                    Name = RubyScriptStepBase<TStep>.NuixRequirementName + feature
                };
        }
    }

    /// <summary>
    /// Creates a new RubyScriptStepFactory.
    /// </summary>
    protected RubyScriptStepFactory()
    {
        _lazyRubyFunction
            = new Lazy<IRubyFunction<TOutput>>(
                () => new RubyFunction<TOutput>(
                    FunctionName,
                    RubyFunctionText,
                    RubyFunctionParameter.GetRubyFunctionParameters<TStep>()
                )
                {
                    RequiredNuixVersion  = RequiredNuixVersion,
                    RequiredNuixFeatures = RequiredFeatures
                }
            );
    }

    private readonly Lazy<IRubyFunction<TOutput>> _lazyRubyFunction;

    /// <summary>
    /// The ruby function to run.
    /// </summary>
    public IRubyFunction<TOutput> RubyFunction => _lazyRubyFunction.Value;

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

    /// <summary>
    /// The Required Nuix Features.
    /// </summary>
    public abstract IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
}

}
