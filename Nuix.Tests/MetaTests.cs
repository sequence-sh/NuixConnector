using System.Reflection;
using Sequence.Core.TestHarness;

namespace Sequence.Connectors.Nuix.Tests;

class MetaTests : MetaTestsBase
{
    /// <inheritdoc />
    public override Assembly StepAssembly { get; } = Assembly.GetAssembly(typeof(IRubyScriptStep))!;

    /// <inheritdoc />
    public override Assembly TestAssembly { get; } = Assembly.GetAssembly(typeof(MetaTests))!;
}
