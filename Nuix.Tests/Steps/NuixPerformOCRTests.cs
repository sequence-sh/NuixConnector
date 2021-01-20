using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public  partial class NuixPerformOCRTests : NuixStepTestBase<NuixPerformOCR, Unit>
{

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
                "Perform OCR",
                DeleteCaseFolder,
                CreateCase,
                AssertCount(0, "sheep"),
                new NuixAddItem
                {
                    Custodian  = Constant("Mark"),
                    Paths      = PoemTextImagePaths,
                    FolderName = Constant("New Folder")
                },
                new NuixPerformOCR { SearchTerm = Constant("*.png") },
                AssertCount(1, "sheep"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );

            yield return new NuixIntegrationTestCase(
                "Perform OCR with named profile",
                DeleteCaseFolder,
                CreateCase,
                AssertCount(0, "sheep"),
                new NuixAddItem
                {
                    Custodian  = Constant("Mark"),
                    Paths      = PoemTextImagePaths,
                    FolderName = Constant("New Folder")
                },
                new NuixPerformOCR
                {
                    SearchTerm = Constant("*.png"), OCRProfileName = Constant("Default")
                },
                AssertCount(1, "sheep"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
