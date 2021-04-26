using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.TestHarness;
using Xunit;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

public class RequirementsTest
{
    private const string FakeConstantPath = "abcd";

    public static readonly TheoryData<(string? expectedError, SCLSettings settings)> TestCases =
        new()
        {
            ("Could not get settings value: Nuix.Features",
             NuixSettings.CreateSettings(
                 FakeConstantPath,
                 new Version(8, 0),
                 true,
                 new List<NuixFeature>()
             )
            ),
            ("Requirement 'Nuix Version 7.0 Features: ANALYSIS' not met.",
             NuixSettings.CreateSettings(
                 FakeConstantPath,
                 new Version(8, 0),
                 true,
                 new List<NuixFeature> { NuixFeature.CASE_CREATION }
             )
            ),
            (null,
             NuixSettings.CreateSettings(
                 FakeConstantPath,
                 new Version(8, 0),
                 true,
                 new List<NuixFeature> { NuixFeature.ANALYSIS }
             )
            )
        };

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestRequirements((string? expectedError, SCLSettings settings) args)
    {
        var process = new NuixSearchAndTag { SearchTerm = Constant("a"), Tag = Constant("c") };

        var result = process.Verify(args.settings);

        if (args.expectedError == null)
            result.ShouldBeSuccessful();
        else
            result.MapError(x => x.AsString).ShouldBeFailure(args.expectedError);
    }
}

}
