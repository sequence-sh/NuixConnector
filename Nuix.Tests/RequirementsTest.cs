using CSharpFunctionalExtensions;
using Reductech.Sequence.Connectors.FileSystem;
using Reductech.Sequence.Core.TestHarness;
using Xunit;

namespace Reductech.Sequence.Connectors.Nuix.Tests;

public class RequirementsTest
{
    private const string FakeConstantPath = "abcd";

    public static readonly TheoryData<(string? expectedError, NuixSettings settings)> TestCases =
        new()
        {
            ("Could not get settings value: Nuix.Features",
             new NuixSettings(
                 FakeConstantPath,
                 new Version(8, 0),
                 true,
                 new List<NuixFeature>()
             )
            ),
            ("Requirement 'Nuix Version 7.0 Features: ANALYSIS' not met.",
             new NuixSettings(
                 FakeConstantPath,
                 new Version(8, 0),
                 true,
                 new List<NuixFeature> { NuixFeature.CASE_CREATION }
             )
            ),
            (null,
             new NuixSettings(
                 FakeConstantPath,
                 new Version(8, 0),
                 true,
                 new List<NuixFeature> { NuixFeature.ANALYSIS }
             )
            )
        };

    [Theory(Skip = "Currently broken")]
    [MemberData(nameof(TestCases))]
    public void TestRequirements((string? expectedError, NuixSettings settings) args)
    {
        var process = new NuixSearchAndTag { SearchTerm = Constant("a"), Tag = Constant("c") };

        var (expectedError, settings) = args;

        var result = process.Verify(
            SettingsHelpers.CreateStepFactoryStore(settings, typeof(DeleteItem).Assembly)
        );

        if (expectedError == null)
            result.ShouldBeSuccessful();
        else
            result.MapError(x => x.AsString).ShouldBeFailure(expectedError);
    }
}
