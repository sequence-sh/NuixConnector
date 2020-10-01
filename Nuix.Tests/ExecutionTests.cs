using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Xunit;
using Reductech.EDR.Core;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    /// <summary>
    /// Tests freezing and execution - much slower
    /// </summary>
    /// <returns></returns>
    [Collection("RequiresNuixLicense")]
    public class ExecutionTests : ExecutionTestCases
    {
        public ExecutionTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(ExecutionTestCases))]
        [Trait(NuixTestCases.Category, NuixTestCases.Integration)]
        public override void Test(string key) => base.Test(key);
    }

    public class ExecutionTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCase> TestCases =>
            NuixTestCases.GetSettingsCombos().Select(x => new ExecutionTestCase(x));


        private class ExecutionTestCase : ITestBaseCase
        {
            public ExecutionTestCase(StepSettingsCombo stepSettingsCombo) => StepSettingsCombo = stepSettingsCombo;

            public StepSettingsCombo StepSettingsCombo { get; }


            /// <inheritdoc />
            public string Name => StepSettingsCombo.ToString();

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                StepSettingsCombo.IsStepCompatible.Should().BeTrue("Step should be compatible");

                StepSettingsCombo.Step.Verify(StepSettingsCombo.Settings).ShouldBeSuccessful(x => x.AsString);

                var loggerFactory = new LoggerFactory(new[] { new XunitLoggerProvider(testOutputHelper) });

                var logger =  loggerFactory.CreateLogger(Name);
                var stateMonad = new StateMonad(logger, StepSettingsCombo.Settings, ExternalProcessRunner.Instance);

                var sw = Stopwatch.StartNew();

                var result = StepSettingsCombo.Step.Run(stateMonad);
                sw.Stop();

                testOutputHelper.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms");

                result.ShouldBeSuccessful(x => x.AsString);
            }
        }
    }
}