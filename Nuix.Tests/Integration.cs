using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;

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

        public class IntegrationTestCase : CaseThatExecutes
        {
            public IntegrationTestCase(string name, IStep<Unit> steps) : base(new List<object>())
            {
                Name = name;
                Steps = steps;
                IgnoreFinalState = true;
            }

            public override string Name { get; }

            public IStep<Unit> Steps { get; }

            /// <inheritdoc />
            public override async Task<IStep> GetStepAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
            {
                var yaml = await Steps.Unfreeze().SerializeToYamlAsync(CancellationToken.None);

                var deserializedStep = YamlMethods.DeserializeFromYaml(yaml,  StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep)));

                deserializedStep.ShouldBeSuccessful(x => x.AsString);

                var unfrozenStep = deserializedStep.Value.TryFreeze().Bind(YamlRunner.ConvertToUnitStep);

                unfrozenStep.ShouldBeSuccessful(x => x.AsString);

                return unfrozenStep.Value;
            }

            /// <inheritdoc />
            public override void CheckUnitResult(Result<Unit, IError> result)
            {
                result.ShouldBeSuccessful(x=>x.AsString);
            }

            /// <inheritdoc />
            public override void CheckOutputResult(Result<TOutput, IError> result)
            {
                result.ShouldBeSuccessful(x=>x.AsString);
            }
        }
    }
}
