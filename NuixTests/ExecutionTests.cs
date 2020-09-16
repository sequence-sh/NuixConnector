using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Xunit;
using Reductech.EDR.Processes;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    /// <summary>
    /// Tests freezing and execution - much slower
    /// </summary>
    /// <returns></returns>
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
        protected override IEnumerable<ITestCase> TestCases =>
            NuixTestCases.GetProcessSettingsCombos().Select(x => new ExecutionTestCase(x));


        private class ExecutionTestCase : ITestCase
        {
            public ExecutionTestCase(ProcessSettingsCombo processSettingsCombo) => ProcessSettingsCombo = processSettingsCombo;

            public ProcessSettingsCombo ProcessSettingsCombo { get; }


            /// <inheritdoc />
            public string Name => ProcessSettingsCombo.ToString();

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                ProcessSettingsCombo.IsProcessCompatible.Should().BeTrue("Process should be compatible");

                ProcessSettingsCombo.Process.Verify(ProcessSettingsCombo.Settings).ShouldBeSuccessful(x => x.AsString);

                var loggerFactory = new LoggerFactory(new[] { new XunitLoggerProvider(testOutputHelper) });

                var logger =  loggerFactory.CreateLogger(Name);
                var processState = new ProcessState(logger, ProcessSettingsCombo.Settings, ExternalProcessRunner.Instance);

                var sw = Stopwatch.StartNew();

                var result = ProcessSettingsCombo.Process.Run(processState);
                sw.Stop();

                testOutputHelper.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms");

                result.ShouldBeSuccessful(x => x.AsString);
            }
        }
    }
}