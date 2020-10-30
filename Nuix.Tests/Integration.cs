using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Xunit;
using Moq;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    [Collection("RequiresNuixLicense")]
    public abstract partial class NuixStepTestBase<TStep, TOutput>
    {
        private IEnumerable<IntegrationTestCase> IntegrationTestCasesWithSettings =>
            from nuixTestCase in NuixTestCases
            from settings in Constants.NuixSettingsList
            where IsVersionCompatible(nuixTestCase.Step, settings.NuixVersion)
            select new IntegrationTestCase(nuixTestCase.Name + settings.NuixVersion, nuixTestCase.Step)
                .WithSettings(settings);

        public IEnumerable<object?[]> IntegrationTestCaseNames =>
            IntegrationTestCasesWithSettings.Select(x => new[] { x.Name });

        [Theory]
        [NonStaticMemberData(nameof(IntegrationTestCaseNames), true)]
        [Trait("Category", "Integration")]
        public async Task Should_behave_as_expected_when_run_integration(string stepCaseName)
        {
            await IntegrationTestCasesWithSettings.FindAndRunAsync(stepCaseName, TestOutputHelper);
        }

        public class NuixIntegrationTestCase
        {
            public NuixIntegrationTestCase(string name, params IStep<Unit>[] steps)
            {
                Name = name;
                Step = new Sequence { Steps = steps };
            }

            public string Name { get; }
            public Sequence Step { get; }
        }

        public class IntegrationTestCase : ICaseThatRuns
        {
            public IntegrationTestCase(string name, IStep<Unit> steps)
            {
                Name = name;
                Steps = steps;
            }

            public string Name { get; }

            public IStep<Unit> Steps { get; }


            /// <inheritdoc />
            public async Task RunCaseAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
            {
                var loggerFactory = new LoggerFactory(new[] { new XunitLoggerProvider(testOutputHelper) });

                var logger = loggerFactory.CreateLogger(Name);
                var factoryStore = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(IRubyScriptStep));

                var stateMonad = new StateMonad(logger, Settings, ExternalProcessRunner.Instance,
                    StepFactoryStoreToUse.Unwrap(factoryStore));

                var yaml = Steps.Unfreeze().SerializeToYaml();

                var deserializedStep = YamlMethods.DeserializeFromYaml(yaml, factoryStore);

                deserializedStep.ShouldBeSuccessful(x => x.AsString);

                var unfrozenStep = deserializedStep.Value.TryFreeze().Bind(YamlRunner.ConvertToUnitStep);

                unfrozenStep.ShouldBeSuccessful(x => x.AsString);

                var sw = Stopwatch.StartNew();

                var result = await unfrozenStep.Value.Run(stateMonad, CancellationToken.None);
                sw.Stop();

                testOutputHelper.WriteLine($"Elapsed: {sw.ElapsedMilliseconds}ms");

                result.ShouldBeSuccessful(x => x.AsString);
            }

            /// <inheritdoc />
            public void AddExternalProcessRunnerAction(Action<Mock<IExternalProcessRunner>> action) =>
                throw new XunitException("Integration tests do not mock the external process runner");

            /// <inheritdoc />
            public Dictionary<VariableName, object> InitialState =>
                throw new XunitException(
                    "Integration tests do not set the initial state"); // { get; } = new Dictionary<VariableName, object>();

            /// <inheritdoc />
            public Dictionary<VariableName, object> ExpectedFinalState =>
                throw new XunitException("Integration tests do not check the final state");

            /// <inheritdoc />
            public ISettings Settings { get; set; } = EmptySettings.Instance;

            public Maybe<StepFactoryStore> StepFactoryStoreToUse { get; set; }
        }
    }
}
