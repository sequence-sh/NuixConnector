using System.IO;

namespace Reductech.Sequence.Connectors.Nuix.Tests.Steps;

public partial class
    NuixExcludeDecryptedItemsTests : NuixStepTestBase<NuixExcludeDecryptedItems, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Exclude decrypted item",
                DeleteCaseFolder,
                AssertCaseDoesNotExist,
                CreateCase,
                new NuixAddItem
                {
                    Custodian = Constant("Mark"),
                    Paths = Array(
                        Path.Combine(
                            Directory.GetCurrentDirectory(),
                            "AllData",
                            "encrypted.pdf"
                        )
                    ),
                    Container        = Constant("EncryptedItems"),
                    PasswordFilePath = PasswordFilePath
                },
                AssertCount(2, "has-exclusion:0 AND \"encrypted\""),
                new NuixExcludeDecryptedItems(),
                AssertCount(1, "has-exclusion:0 AND \"encrypted\""),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}
