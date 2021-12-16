using System.Linq;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta;

/// <summary>
/// A step that runs a ruby script against NUIX
/// </summary>
public abstract class RubyScriptStepFactory<TStep, TOutput> : SimpleStepFactory<TStep, TOutput>,
                                                              IRubyScriptStepFactory<TOutput> where TOutput : ISCLObject
    where TStep : IRubyScriptStep<TOutput>, new()
{
    private static string NuixConnectorName { get; } = typeof(NuixAddItem).Assembly.GetName().Name!;

    /// <inheritdoc />
    public override IEnumerable<Requirement> Requirements
    {
        get
        {
            var minVersion = NuixVersionHelper.DefaultRequiredVersion
                           > RubyFunction.RequiredNuixVersion
                ? NuixVersionHelper.DefaultRequiredVersion
                : RubyFunction.RequiredNuixVersion;

            yield return new VersionRequirement(
                NuixConnectorName,
                RubyScriptStepBase<SCLNull>.NuixVersionKey,
                minVersion
            );

            var requiredFeatures =
                RubyFunction.RequiredNuixFeatures.Select(x => x.ToString()).ToList();

            if (requiredFeatures.Any())
                yield return new FeatureRequirement(
                    NuixConnectorName,
                    RubyScriptStepBase<SCLNull>.NuixFeaturesKey,
                    requiredFeatures
                );
        }
    }

    /// <summary>
    /// Creates a new RubyScriptStepFactory.
    /// </summary>
    protected RubyScriptStepFactory()
    {
        _lazyRubyFunction
            = new Lazy<RubyFunction<TOutput>>(
                () => new RubyFunction<TOutput>(
                    FunctionName,
                    RubyFunctionText,
                    RubyFunctionParameter.GetRubyFunctionParameters<TStep>(),
                    RequiredNuixVersion,
                    RequiredFeatures,
                    RequiredHelpers!
                )
            );
    }

    private readonly Lazy<RubyFunction<TOutput>> _lazyRubyFunction;

    /// <summary>
    /// The ruby function to run.
    /// </summary>
    public RubyFunction<TOutput> RubyFunction => _lazyRubyFunction.Value;

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

    /// <summary>
    /// Any helper functions required for this Step to execute.
    /// </summary>
    public virtual IReadOnlyCollection<IRubyHelper>? RequiredHelpers { get; } = null;
}
