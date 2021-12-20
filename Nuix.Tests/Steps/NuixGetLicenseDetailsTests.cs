using Reductech.Sequence.Core.Steps;

namespace Reductech.Sequence.Connectors.Nuix.Tests.Steps;

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
                    Variable = VariableName.Item, Value = new NuixGetLicenseDetails()
                },
                AssertPropertyValueEquals("Name",           Constant("enterprise-workstation")),
                AssertPropertyValueEquals("Workers",        Constant(2)),
                AssertPropertyValueEquals("AuditThreshold", Constant(5000000000.0)),
                AssertPropertyValueEquals("IsValid",        Constant(true)),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}
