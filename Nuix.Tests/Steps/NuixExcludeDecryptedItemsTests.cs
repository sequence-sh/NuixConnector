using System.Collections.Generic;
using System.IO;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

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

}
