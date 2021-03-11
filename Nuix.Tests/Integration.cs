using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit;
using Xunit.Abstractions;
using Unit = Reductech.EDR.Core.Util.Unit;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

[Collection("RequiresNuixLicense")]
public abstract partial class NuixStepTestBase<TStep, TOutput>
{
    [AutoTheory.GenerateAsyncTheory("NuixIntegration", Category = "Integration")]
    public IEnumerable<IntegrationTestCase> IntegrationTestCasesWithSettings =>
        from nuixTestCase in NuixTestCases
        from settings in Constants.NuixSettingsList
        where IsVersionCompatible(
            nuixTestCase.Step,
            NuixSettings.TryGetNuixVersion(settings).Value
        ) //&& false //uncomment to disable integration tests
        select new IntegrationTestCase(
            // Name needs to have nuix version in parentheses for ci script to build summary
            $"{nuixTestCase.Name} ({NuixSettings.TryGetNuixVersion(settings).Value})",
            nuixTestCase.Step
        ).WithSettings(settings);

    public class NuixIntegrationTestCase
    {
        public NuixIntegrationTestCase(string name, params IStep<Unit>[] steps)
        {
            Name = name;

            Step = new Sequence<Unit>
            {
                InitialSteps = steps, FinalStep = new NuixCloseConnection()
            };
        }

        public string Name { get; }
        public Sequence<Unit> Step { get; }
    }

    public record IntegrationTestCase : CaseThatExecutes
    {
        public IntegrationTestCase(string name, IStep<Unit> steps) : base(name, new List<string>())
        {
            Steps              = steps;
            IgnoreFinalState   = true;
            IgnoreLoggedValues = true;
        }

        /// <inheritdoc />
        public override LogLevel OutputLogLevel => OutputLogLevel1;

        public LogLevel OutputLogLevel1 { get; set; } = LogLevel.Debug;

        public IStep<Unit> Steps { get; }

        /// <inheritdoc />
        public override async Task<IStep> GetStepAsync(ITestOutputHelper testOutputHelper)
        {
            await Task.CompletedTask;
            var yaml = Steps.Serialize();

            testOutputHelper.WriteLine(yaml);

            var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep));

            var deserializedStep = SCLParsing.ParseSequence(yaml);

            deserializedStep.ShouldBeSuccessful(x => x.AsString);

            var unfrozenStep = deserializedStep.Value.TryFreeze(TypeReference.Any.Instance, sfs);

            unfrozenStep.ShouldBeSuccessful(x => x.AsString);

            return unfrozenStep.Value;
        }

        /// <inheritdoc />
        public override void CheckUnitResult(Result<Unit, IError> result)
        {
            result.ShouldBeSuccessful(x => x.AsString);
        }

        /// <inheritdoc />
        public override void CheckOutputResult(Result<TOutput, IError> result)
        {
            result.ShouldBeSuccessful(x => x.AsString);
        }

        /// <inheritdoc />
        public override async Task<StateMonad> GetStateMonad(
            MockRepository mockRepository,
            ILogger logger)
        {
            var baseMonad = await base.GetStateMonad(mockRepository, logger);

            return new StateMonad(
                baseMonad.Logger,
                baseMonad.Settings,
                baseMonad.StepFactoryStore,
                new ExternalContext(
                    FileSystemAdapter.Default,
                    ExternalProcessRunner.Instance,
                    baseMonad.ExternalContext.Console
                ),
                baseMonad.SequenceMetadata
            );
        }
    }
}

}
