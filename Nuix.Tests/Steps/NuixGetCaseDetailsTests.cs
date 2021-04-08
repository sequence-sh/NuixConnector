using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixGetCaseDetailsTests : NuixStepTestBase<NuixGetCaseDetails, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Return Nuix case details",
                SetupCase,
                new SetVariable<Entity>
                {
                    Variable = VariableName.Entity, Value = new NuixGetCaseDetails()
                },
                AssertPropertyValueEquals("Name",         Constant("Integration Test Case")),
                AssertPropertyValueEquals("Location",     CasePath),
                AssertPropertyValueEquals("Investigator", Constant("Mark")),
                AssertPropertyValueEquals("IsCompound",   Constant(false)),
                new AssertTrue
                {
                    Boolean = new Equals<StringStream>
                    {
                        Terms = new ArrayNew<StringStream>
                        {
                            Elements = new List<IStep<StringStream>>
                            {
                                Constant("2020-10-20"),
                                new EntityGetValue<StringStream>
                                {
                                    Entity = new GetVariable<Entity>
                                    {
                                        Variable = VariableName.Entity
                                    },
                                    Property = Constant("EarliestDate")
                                }
                            }
                        }
                    }
                },
                CleanupCase
            );
        }
    }
}

}
