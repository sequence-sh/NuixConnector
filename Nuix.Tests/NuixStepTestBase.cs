using System;
using System.Collections.Generic;
using System.Linq;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

public abstract partial class NuixStepTestBase<TStep, TOutput> : StepTestBase<TStep, TOutput>
    where TStep : class, IRubyScriptStep<TOutput>, new()
{
    public const string TestNuixPath = "TestPath";

    private static bool IsVersionCompatible(IStep step, Version nuixVersion)
    {
        var features = Enum.GetValues(typeof(NuixFeature)).Cast<NuixFeature>().ToHashSet();

        var settings = NuixSettings.CreateSettings(
            TestNuixPath,
            nuixVersion,
            true,
            features
        );

        var r = step.Verify(settings);
        return r.IsSuccess;
    }

    public SCLSettings UnitTestSettings
    {
        get
        {
            var instance = new TStep();
            var factory  = instance.RubyScriptStepFactory;

            return NuixSettings.CreateSettings(
                TestNuixPath,
                factory.RequiredNuixVersion,
                true,
                factory.RequiredFeatures
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
                var caseWithSettings = baseErrorCase.WithSettings(UnitTestSettings);
                yield return caseWithSettings;
            }
        }
    }

    protected abstract IEnumerable<NuixIntegrationTestCase> NuixTestCases { get; }
}

}
