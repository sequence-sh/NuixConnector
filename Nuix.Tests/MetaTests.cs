using System.Reflection;
using Reductech.Sequence.Core.TestHarness;

namespace Reductech.Sequence.Connectors.Nuix.Tests;

class MetaTests : MetaTestsBase
{
    /// <inheritdoc />
    public override Assembly StepAssembly { get; } = Assembly.GetAssembly(typeof(IRubyScriptStep))!;

    /// <inheritdoc />
    public override Assembly TestAssembly { get; } = Assembly.GetAssembly(typeof(MetaTests))!;
}
