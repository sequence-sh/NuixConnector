using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
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
        public override Task Test(string key) => base.Test(key);
    }


    public class ScriptCompositionTestCases : TestBaseParallel
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCaseParallel> TestCases =>
            NuixTestCases.GetSettingsCombos().Select(x => new ScriptCompositionTest(x));

        private class ScriptCompositionTest : ITestBaseCaseParallel
        {
            public ScriptCompositionTest(StepSettingsCombo stepSettingsCombo) => StepSettingsCombo = stepSettingsCombo;

            /// <inheritdoc />
            public string Name => StepSettingsCombo.ToString();

            public StepSettingsCombo StepSettingsCombo { get; }

            /// <inheritdoc />
            public async Task ExecuteAsync(ITestOutputHelper testOutputHelper)
            {
                var externalProcessRunner = new TestExternalProcessRunner(testOutputHelper);


                StepSettingsCombo.IsStepCompatible.Should().BeTrue("Step should be compatible");

                StepSettingsCombo.Step.Verify(StepSettingsCombo.Settings).ShouldBeSuccessful(x => x.AsString);
                var factoryStore = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(IRubyScriptStep));

                var stateMonad = new StateMonad(NullLogger.Instance, StepSettingsCombo.Settings, externalProcessRunner, factoryStore);

                var result = await StepSettingsCombo.Step.Run(stateMonad, CancellationToken.None);
                result.ShouldBeSuccessful(x => x.AsString);

                externalProcessRunner.TimesCalled.Should().Be(1, "exactly one script should be called");
            }
        }

        internal class TestExternalProcessRunner : IExternalProcessRunner
        {
            public TestExternalProcessRunner(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

            public ITestOutputHelper TestOutputHelper { get; }

            /// <inheritdoc />
            public async Task<Result<Unit, IRunErrors>> RunExternalProcess(string processPath, ILogger logger, string callingProcessName, IErrorHandler errorHandler, IEnumerable<string> arguments)
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
