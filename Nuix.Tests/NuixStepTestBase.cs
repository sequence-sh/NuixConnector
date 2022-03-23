using Reductech.Sequence.Connectors.FileSystem;
using Reductech.Sequence.Connectors.FileSystem.Steps;
using Reductech.Sequence.Core.TestHarness;

namespace Reductech.Sequence.Connectors.Nuix.Tests;

public abstract partial class NuixStepTestBase<TStep, TOutput> : StepTestBase<TStep, TOutput>
    where TStep : class, IRubyScriptStep<TOutput>, new()
    where TOutput : ISCLObject
{
    public const string TestNuixPath = "TestPath";

    public StepFactoryStore UnitTestSettings
    {
        get
        {
            var instance = new TStep();
            var factory  = instance.RubyScriptStepFactory;

            Version requiredVersion = new(5, 0);

            if (factory.RequiredNuixVersion > requiredVersion)
                requiredVersion = factory.RequiredNuixVersion;

            return SettingsHelpers.CreateStepFactoryStore(
                new NuixSettings(
                    TestNuixPath,
                    requiredVersion,
                    true,
                    factory.RequiredFeatures
                ),
                typeof(DeleteItem).Assembly
            );
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            foreach (var baseErrorCase in base.ErrorCases)
            {
                var caseWithSettings = baseErrorCase.WithStepFactoryStore(UnitTestSettings);
                yield return caseWithSettings;
            }
        }
    }

    protected abstract IEnumerable<NuixIntegrationTestCase> NuixTestCases { get; }
}
