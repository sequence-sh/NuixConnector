using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixGetAuditedSizeTests : NuixStepTestBase<NuixGetAuditedSize, double>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Ingest items and calculate audited size",
                DeleteCaseFolder,
                CreateCase,
                new NuixAddItem
                {
                    Custodian = Constant("Mark"),
                    Paths     = DataPaths,
                    Container = Constant("New Folder"),
                    ProcessingSettings = Constant(
                        Core.Entity.Create(("calculateAuditedSize", true))
                    )
                },
                new AssertTrue
                {
                    // TODO: Comparing the two as a string is a hack and will need to be
                    // reverted back to double once the upstream bug in Core has been fixed
                    Boolean = new Equals<StringStream>()
                    {
                        Terms = new ArrayNew<StringStream>()
                        {
                            Elements = new List<IStep<StringStream>>()
                            {
                                Constant("799"),
                                new StringInterpolate
                                {
                                    Strings = new List<IStep>
                                    {
                                        new NuixGetAuditedSize()
                                    }
                                }
                            }
                        }
                    }
                },
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
