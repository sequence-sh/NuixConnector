using System;
using System.Collections.Generic;
using Reductech.EDR.Connectors.FileSystem;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

public abstract partial class NuixStepTestBase<TStep, TOutput> : StepTestBase<TStep, TOutput>
    where TStep : class, IRubyScriptStep<TOutput>, new()
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

}
