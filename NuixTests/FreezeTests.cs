using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Reductech.EDR.Processes.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    /// <summary>
    /// Tests just the freezing and unfreezing of the processes. Suitable as a unit test.
    /// </summary>
    /// <returns></returns>
    public class FreezeTests : FreezeTestCases
    {
        public FreezeTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;


        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(FreezeTestCases))]
        public override void Test(string key)
        {
            base.Test(key);
        }
    }


    public class FreezeTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases => NuixTestCases.GetProcessSettingsCombos()
            .Select(c => new FreezeTestCase(c));


        private class FreezeTestCase : ITestCase
        {
            public FreezeTestCase(ProcessSettingsCombo processSettingsCombo) => ProcessSettingsCombo = processSettingsCombo;

            public ProcessSettingsCombo ProcessSettingsCombo { get; }


            /// <inheritdoc />
            public string Name => ProcessSettingsCombo.ToString();

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                ProcessSettingsCombo.IsProcessCompatible.Should().BeTrue("Process should be compatible");


                var unfrozen = ProcessSettingsCombo.Process.Unfreeze();
                var freezeResult = unfrozen.TryFreeze();
                freezeResult.ShouldBeSuccessful();
                var verifyResult = freezeResult.Value.Verify(ProcessSettingsCombo.Settings);


                verifyResult.ShouldBeSuccessful(x => x.AsString);
            }
        }
    }
}