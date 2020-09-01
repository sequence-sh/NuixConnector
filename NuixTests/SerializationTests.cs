using System.Collections.Generic;
using System.Linq;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Serialization;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public class SerializationTests : SerializationTestCases
    {
        public SerializationTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(SerializationTestCases))]
        public override void Test(string key)
        {
            base.Test(key);
        }
    }

    public class SerializationTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases =>
            NuixTestCases.GetProcessSettingsCombos().Select(x => new SerializationTest(x));

        private class SerializationTest : ITestCase
        {
            public SerializationTest(ProcessSettingsCombo processSettingsCombo) => ProcessSettingsCombo = processSettingsCombo;

            /// <inheritdoc />
            public string Name => ProcessSettingsCombo.ToString();

            public ProcessSettingsCombo ProcessSettingsCombo { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var unfrozen = ProcessSettingsCombo.Process.Unfreeze();

                var yaml = unfrozen.SerializeToYaml();

                testOutputHelper.WriteLine(yaml);

                var pfs = ProcessFactoryStore.CreateUsingReflection(typeof(RubyScriptProcessUnit),
                    typeof(CompoundFreezableProcess));

                var r = YamlMethods.DeserializeFromYaml(yaml, pfs);

                r.ShouldBeSuccessful();
            }
        }
    }
}