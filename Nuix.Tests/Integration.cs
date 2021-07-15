using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutoTheory;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Connectors.FileSystem;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Connectors;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

[Collection("RequiresNuixLicense")]
public abstract partial class NuixStepTestBase<TStep, TOutput>
{
    [GenerateAsyncTheory("NuixIntegration", Category = "Integration")]
    public IEnumerable<IntegrationTestCase> IntegrationTestCasesWithSettings
    {
        get
        {
            foreach (NuixIntegrationTestCase nuixTestCase in NuixTestCases)
            {
                var atLeastOne = false;

                foreach (NuixSettings settings in Constants.NuixSettingsList)
                {
                    if (IsVersionCompatible(
                        nuixTestCase.Step,
                        settings.Version!
                    ))
                    {
                        yield return new IntegrationTestCase(
                            // Name needs to have nuix version in parentheses for ci script to build summary
                            $"{nuixTestCase.Name} ({settings.Version})",
                            nuixTestCase.Step
                        ).WithStepFactoryStore(
                            SettingsHelpers.CreateStepFactoryStore(
                                settings,
                                typeof(DeleteItem).Assembly
                            )
                        );

                        atLeastOne = true;
                    }
                }

                if (!atLeastOne)
                    throw new XunitException(
                        $"Test '{nuixTestCase.Name}' has no compatible settings"
                    );
            }
        }
    }

    private static bool IsVersionCompatible(IStep step, Version nuixVersion)
    {
        var features = Enum.GetValues(typeof(NuixFeature)).Cast<NuixFeature>().ToHashSet();

        var settings = new NuixSettings(
            TestNuixPath,
            nuixVersion,
            true,
            features
        );

        var sfs = SettingsHelpers.CreateStepFactoryStore(settings, typeof(DeleteItem).Assembly);

        var r = step.Verify(sfs);
        return r.IsSuccess;
    }

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

            var connectorInjections = new IConnectorInjection[]
            {
                new ConnectorInjection(), new FileSystem.ConnectorInjection()
            };

            foreach (var connectorInjection in connectorInjections)
            {
                var injectedContextsResult = connectorInjection.TryGetInjectedContexts();
                injectedContextsResult.ShouldBeSuccessful();

                foreach (var (contextName, context) in injectedContextsResult.Value)
                {
                    ExternalContextSetupHelper.AddContextObject(contextName, context);
                }
            }
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

            var sfs = SettingsHelpers.CreateStepFactoryStore(
                null,
                Assembly.GetAssembly(typeof(DeleteItem))!
            );

            var deserializedStep = SCLParsing.TryParseStep(yaml);

            deserializedStep.ShouldBeSuccessful();

            var unfrozenStep = deserializedStep.Value.TryFreeze(SCLRunner.RootCallerMetadata, sfs);

            unfrozenStep.ShouldBeSuccessful();

            return unfrozenStep.Value;
        }

        /// <inheritdoc />
        public override void CheckUnitResult(Result<Unit, IError> result)
        {
            result.ShouldBeSuccessful();
        }

        /// <inheritdoc />
        public override void CheckOutputResult(Result<TOutput, IError> result)
        {
            result.ShouldBeSuccessful();
        }

        /// <inheritdoc />
        public override async Task<StateMonad> GetStateMonad(
            MockRepository mockRepository,
            ILogger logger)
        {
            var baseMonad = await base.GetStateMonad(mockRepository, logger);

            return new StateMonad(
                baseMonad.Logger,
                baseMonad.StepFactoryStore,
                new ExternalContext(
                    ExternalProcessRunner.Instance,
                    baseMonad.ExternalContext.Console,
                    baseMonad.ExternalContext.InjectedContexts
                ),
                baseMonad.SequenceMetadata
            );
        }
    }
}

}
