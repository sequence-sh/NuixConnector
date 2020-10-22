using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Xunit;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
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
        public override Task Test(string key) => base.Test(key);
    }

    public class ExecutionTestCases : TestBaseParallel
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCaseParallel> TestCases =>
            NuixTestCases.GetSettingsCombos().Select(x => new ExecutionTestCase(x));


        private class ExecutionTestCase : ITestBaseCaseParallel
        {
            public ExecutionTestCase(StepSettingsCombo stepSettingsCombo) => StepSettingsCombo = stepSettingsCombo;

            public StepSettingsCombo StepSettingsCombo { get; }


            /// <inheritdoc />
            public string Name => StepSettingsCombo.ToString();

            /// <inheritdoc />
            public async Task ExecuteAsync(ITestOutputHelper testOutputHelper)
            {
                StepSettingsCombo.IsStepCompatible.Should().BeTrue("Step should be compatible");

                StepSettingsCombo.Step.Verify(StepSettingsCombo.Settings).ShouldBeSuccessful(x => x.AsString);

                var loggerFactory = new LoggerFactory(new[] { new XunitLoggerProvider(testOutputHelper) });

                var logger =  loggerFactory.CreateLogger(Name);
                var factoryStore = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(IRubyScriptStep));

                var stateMonad = new StateMonad(logger, StepSettingsCombo.Settings, ExternalProcessRunner.Instance, factoryStore);

                var sw = Stopwatch.StartNew();

                var result = await StepSettingsCombo.Step.Run(stateMonad, CancellationToken.None);
                sw.Stop();

                testOutputHelper.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms");

                result.ShouldBeSuccessful(x => x.AsString);
            }
        }
    }
}