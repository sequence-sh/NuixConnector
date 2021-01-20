using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Moq;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit;
using Xunit.Abstractions;

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
            settings.NuixVersion
        ) //&& false //uncomment to disable integration tests
        select new IntegrationTestCase(nuixTestCase.Name + settings.NuixVersion, nuixTestCase.Step)
            .WithSettings(settings);

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

        public IStep<Unit> Steps { get; }

        /// <inheritdoc />
        public override async Task<IStep> GetStepAsync(ITestOutputHelper testOutputHelper)
        {
            await Task.CompletedTask;
            var yaml = Steps.Serialize();

            var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(TStep));

            var deserializedStep = SCLParsing.ParseSequence(yaml);

            deserializedStep.ShouldBeSuccessful(x => x.AsString);

            var unfrozenStep = deserializedStep.Value.TryFreeze(sfs);

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
        public override IFileSystemHelper GetFileSystemHelper(MockRepository mockRepository) =>
            FileSystemHelper.Instance;

        /// <inheritdoc />
        public override IExternalProcessRunner GetExternalProcessRunner(
            MockRepository mockRepository) => ExternalProcessRunner.Instance;
    }
}

}
