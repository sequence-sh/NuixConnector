using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public class ScriptCompositionTests : ScriptCompositionTestCases
    {
        public ScriptCompositionTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;


        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(ScriptCompositionTestCases))]
        [Trait(NuixTestCases.Category, NuixTestCases.Integration)]
        public override void Test(string key)
        {
            base.Test(key);
        }
    }


    public class ScriptCompositionTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestCase> TestCases =>
            NuixTestCases.GetProcessSettingsCombos().Select(x => new ScriptCompositionTest(x));

        private class ScriptCompositionTest : ITestCase
        {
            public ScriptCompositionTest(ProcessSettingsCombo processSettingsCombo) => ProcessSettingsCombo = processSettingsCombo;

            /// <inheritdoc />
            public string Name => ProcessSettingsCombo.ToString();

            public ProcessSettingsCombo ProcessSettingsCombo { get; }

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var externalProcessRunner = new TestExternalProcessRunner(testOutputHelper);


                ProcessSettingsCombo.IsProcessCompatible.Should().BeTrue("Process should be compatible");

                ProcessSettingsCombo.Process.Verify(ProcessSettingsCombo.Settings).ShouldBeSuccessful(x => x.AsString);

                var processState = new ProcessState(NullLogger.Instance, ProcessSettingsCombo.Settings, externalProcessRunner);

                var result = ProcessSettingsCombo.Process.Run(processState);
                result.ShouldBeSuccessful(x => x.AsString);

                externalProcessRunner.TimesCalled.Should().Be(1, "exactly one script should be called");
            }
        }

        internal class TestExternalProcessRunner : IExternalProcessRunner
        {
            public TestExternalProcessRunner(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

            public ITestOutputHelper TestOutputHelper { get; }

            /// <inheritdoc />
            public async Task<Result<Unit, IRunErrors>> RunExternalProcess(string processPath, ILogger logger, string callingProcessName, IEnumerable<string> arguments)
            {
                var args = arguments.ToList();

                args[0].Should().Be("-licencesourcetype");
                args[1].Should().Be("dongle");
                var scriptPath = args[2];

                File.Exists(scriptPath).Should().BeTrue("third argument should be a path to a document");

                var data = await File.ReadAllTextAsync(scriptPath);
                TestOutputHelper.WriteLine(data);

                logger.LogInformation(ScriptGenerator.UnitSuccessToken);

                TimesCalled++;

                return Unit.Default;
            }

            public int TimesCalled { get; private set; } = 0;
        }
    }

}
