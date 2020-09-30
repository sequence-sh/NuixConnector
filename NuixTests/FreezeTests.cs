using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Reductech.EDR.Core.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    /// <summary>
    /// Tests just the freezing and unfreezing of the steps. Suitable as a unit test.
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
        protected override IEnumerable<ITestCase> TestCases => NuixTestCases.GetSettingsCombos()
            .Select(c => new FreezeTestCase(c));


        private class FreezeTestCase : ITestCase
        {
            public FreezeTestCase(StepSettingsCombo stepSettingsCombo) => StepSettingsCombo = stepSettingsCombo;

            public StepSettingsCombo StepSettingsCombo { get; }


            /// <inheritdoc />
            public string Name => StepSettingsCombo.ToString();

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                StepSettingsCombo.IsStepCompatible.Should().BeTrue("Step should be compatible");


                var unfrozen = StepSettingsCombo.Step.Unfreeze();
                var freezeResult = unfrozen.TryFreeze();
                freezeResult.ShouldBeSuccessful();
                var verifyResult = freezeResult.Value.Verify(StepSettingsCombo.Settings);


                verifyResult.ShouldBeSuccessful(x => x.AsString);
            }
        }
    }
}