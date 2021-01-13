using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Steps;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public class NuixGetItemPropertiesTests : NuixStepTestBase<NuixGetItemProperties, StringStream>
{
    /// <inheritdoc />
    public NuixGetItemPropertiesTests(ITestOutputHelper testOutputHelper) :
        base(testOutputHelper) { }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get { yield break; }
    }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Get Item Properties",
                DeleteCaseFolder,
                DeleteOutputFolder,
                CreateOutputFolder,
                CreateCase,
                AddData,
                new FileWrite
                {
                    Path =
                        new PathCombine { Paths = Array(OutputFolder, "ItemProperties.txt") },
                    Stream = new NuixGetItemProperties
                    {
                        PropertyRegex = Constant("(.+)"), SearchTerm = Constant("*")
                    }
                },
                AssertFileContains(
                    OutputFolder,
                    "ItemProperties.txt",
                    "Character Set	UTF-8	New Folder/data/Jellyfish.txt"
                ),
                new NuixCloseConnection(),
                DeleteCaseFolder,
                DeleteOutputFolder
            );
        }
    }
}

}
