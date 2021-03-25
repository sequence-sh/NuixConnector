using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixGetLicenseDetailsTests : NuixStepTestBase<NuixGetLicenseDetails, Entity>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Return Nuix license details",
                DeleteCaseFolder,
                CreateCase,
                new SetVariable<Entity>
                {
                    Variable = VariableName.Entity, Value = new NuixGetLicenseDetails()
                },
                new AssertTrue
                {
                    Boolean = new Equals<StringStream>
                    {
                        Terms = new ArrayNew<StringStream>
                        {
                            Elements = new List<IStep<StringStream>>
                            {
                                Constant("enterprise-workstation"),
                                new EntityGetValue<StringStream>
                                {
                                    Entity = new GetVariable<Entity>
                                    {
                                        Variable = VariableName.Entity
                                    },
                                    Property = Constant("Name")
                                }
                            }
                        }
                    }
                },
                new AssertTrue
                {
                    Boolean = new Equals<int>
                    {
                        Terms = new ArrayNew<int>
                        {
                            Elements = new List<IStep<int>>
                            {
                                Constant(2),
                                new EntityGetValue<int>
                                {
                                    Entity = new GetVariable<Entity>
                                    {
                                        Variable = VariableName.Entity
                                    },
                                    Property = Constant("Workers")
                                }
                            }
                        }
                    }
                },
                new AssertTrue
                {
                    Boolean = new Equals<double>
                    {
                        Terms = new ArrayNew<double>
                        {
                            Elements = new List<IStep<double>>
                            {
                                Constant(5000000000.0),
                                new EntityGetValue<double>
                                {
                                    Entity = new GetVariable<Entity>
                                    {
                                        Variable = VariableName.Entity
                                    },
                                    Property = Constant("AuditThreshold")
                                }
                            }
                        }
                    }
                },
                new AssertTrue
                {
                    Boolean = new EntityGetValue<bool>
                    {
                        Entity   = new GetVariable<Entity> { Variable = VariableName.Entity },
                        Property = Constant("IsValid")
                    }
                },
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
