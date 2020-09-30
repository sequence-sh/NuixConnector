using System.Collections.Generic;
using System.Linq;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
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
        public override void Test(string key) => base.Test(key);
    }

    public class SerializationTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases =>
            NuixTestCases.GetSettingsCombos().Select(x => new SerializationTest(x));

        private class SerializationTest : ITestCase
        {
            public SerializationTest(StepSettingsCombo stepSettingsCombo) => StepSettingsCombo = stepSettingsCombo;

            /// <inheritdoc />
            public string Name => StepSettingsCombo.ToString();

            public StepSettingsCombo StepSettingsCombo { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var unfrozen = StepSettingsCombo.Step.Unfreeze();

                var yaml = unfrozen.SerializeToYaml();

                testOutputHelper.WriteLine(yaml);

                var pfs = StepFactoryStore.CreateUsingReflection(typeof(RubyScriptStepUnit),
                    typeof(CompoundFreezableStep));

                var r = YamlMethods.DeserializeFromYaml(yaml, pfs);

                r.ShouldBeSuccessful();
            }
        }
    }
}