using System.Reflection;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.TestHarness;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

class MetaTests : MetaTestsBase
{
    /// <inheritdoc />
    public override Assembly StepAssembly { get; } = Assembly.GetAssembly(typeof(IRubyScriptStep))!;

    /// <inheritdoc />
    public override Assembly TestAssembly { get; } = Assembly.GetAssembly(typeof(MetaTests))!;
}

}
