using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoTheory;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Connectors;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Xunit;
using Xunit.Abstractions;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps;

[UseTestOutputHelper]
[Collection("RequiresNuixLicense")]
public partial class NuixRunScriptTests : StepTestBase<NuixRunScript, StringStream>
{
    protected override IEnumerable<ErrorCase> ErrorCases
    {
        get
        {
            return from errorCase in base.ErrorCases
                   where
                       !errorCase.Name.Contains(
                           "EntityStreamParameter"
                       ) //Skip this case because the failure happens too late to be tested here (it is tested in NuixConnection)
                   select errorCase.WithStepFactoryStore(UnitTestSettings);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new RunScriptStepCase(
                    "Run Script, no entity Stream",
                    new NuixRunScript
                    {
                        FunctionName          = Constant("Test Script"),
                        ScriptText            = Constant("Lorem Ipsum"),
                        EntityStreamParameter = null,
                        Parameters            = Constant(Entity.Create(("param1", "ABC")))
                    },
                    "Hello World",
                    new List<ExternalProcessAction>
                    {
                        new(
                            new ConnectionCommand
                            {
                                Command            = "test_Script",
                                FunctionDefinition = "Lorem Ipsum",
                                Arguments =
                                    new Dictionary<string, object> { { "param1", "ABC" } }
                            },
                            new ConnectionOutput
                            {
                                Log = new ConnectionOutputLog
                                {
                                    Message = "Log Message", Severity = "info"
                                }
                            },
                            new ConnectionOutput
                            {
                                Result = new ConnectionOutputResult { Data = "Hello World" }
                            }
                        )
                    },
                    "Log Message"
                ).WithScriptExists()
                .WithStepFactoryStore(UnitTestSettings);

            yield return new RunScriptStepCase(
                    "Run Script with entity Stream",
                    new NuixRunScript
                    {
                        FunctionName = Constant("test_Script"),
                        ScriptText   = Constant("Lorem Ipsum"),
                        EntityStreamParameter =
                            Array(Entity.Create(("Foo", "a")), Entity.Create(("Foo", "b"))),
                        Parameters = Constant(Entity.Create(("param1", "ABC")))
                    },
                    @"[{""Foo"":""a""},{""Foo"":""b""}]",
                    new List<ExternalProcessAction>
                    {
                        new(
                            new ConnectionCommand
                            {
                                Command            = "test_Script",
                                FunctionDefinition = "Lorem Ipsum",
                                Arguments =
                                    new Dictionary<string, object> { { "param1", "ABC" } },
                                IsStream = true
                            },
                            new ConnectionOutput
                            {
                                Log = new ConnectionOutputLog
                                {
                                    Message = "Log Message", Severity = "info"
                                }
                            }
                        )
                    },
                    "Log Message"
                ).WithScriptExists()
                .WithStepFactoryStore(UnitTestSettings);
        }
    }

    public const string TestNuixPath = "TestPath";

    public static List<NuixFeature> AllNuixFeatures =>
        Enum.GetValues(typeof(NuixFeature)).Cast<NuixFeature>().ToList();

    public static StepFactoryStore UnitTestSettings => SettingsHelpers.CreateStepFactoryStore(
        new NuixSettings(
            TestNuixPath,
            new Version(8, 2),
            true,
            AllNuixFeatures
        )
    );

    public static StepFactoryStore IntegrationTestSettings =>
        SettingsHelpers.CreateStepFactoryStore(
            new NuixSettings(
                Path.Combine(Constants.Nuix8Path, Constants.NuixConsoleExe),
                new Version(8, 8),
                true,
                AllNuixFeatures
            )
        );

    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestScriptWithStream_Integration()
    {
        var stepCase = new IntegrationTestCase(
            "Case with stream",
            new NuixRunScript
            {
                FunctionName = Constant("test_Script2"),
                ScriptText =
                    Constant(
                        "log param1\r\nlog datastream.pop\r\nlog datastream.pop\r\nreturn param2"
                    ),
                EntityStreamParameter =
                    Array(Entity.Create(("Foo", "a")), Entity.Create(("Foo", "b"))),
                Parameters = Constant(Entity.Create(("param1", "ABC"), ("param2", "DEF")))
            },
            "DEF",
            "NuixConnectorScript starting",
            "ABC",
            "{\"Foo\":\"a\"}",
            "{\"Foo\":\"b\"}",
            "NuixConnectorScript finished"
        ).WithStepFactoryStore(IntegrationTestSettings);

        await stepCase.RunAsync(TestOutputHelper);
    }

    [Fact]
    [Trait("Category", "Integration")]
    public async Task TestScriptWithNoStream_Integration()
    {
        var stepCase = new IntegrationTestCase(
            "Case without stream",
            new NuixRunScript
            {
                FunctionName = Constant("test_Script"),
                ScriptText = Constant("log param1\r\nreturn param2"),
                EntityStreamParameter = null,
                Parameters = Constant(Entity.Create(("param1", "ABC"), ("param2", "DEF")))
            },
            "DEF",
            "NuixConnectorScript starting",
            "ABC",
            "NuixConnectorScript finished"
        ).WithStepFactoryStore(IntegrationTestSettings);

        await stepCase.RunAsync(TestOutputHelper);
    }

    public record RunScriptStepCase : StepCase
    {
        public RunScriptStepCase(
            string name,
            NuixRunScript step,
            string expectedOutput,
            IReadOnlyCollection<ExternalProcessAction> externalProcessActions,
            params string[] expectedLogValues)
            : base(name, step, expectedOutput, expectedLogValues)
        {
            ExternalProcessActions = externalProcessActions;
            IgnoreFinalState       = true;
        }

        public IReadOnlyCollection<ExternalProcessAction> ExternalProcessActions { get; }

        /// <inheritdoc />
        public override async Task<StateMonad> GetStateMonad(
            IExternalContext externalContext,
            ILogger logger)
        {
            var externalProcessMock = new ExternalProcessMock(
                1,
                ExternalProcessActions.ToArray()
            );

            var newExternalContext = new ExternalContext(
                externalProcessMock,
                externalContext.RestClientFactory,
                externalContext.Console,
                externalContext.InjectedContexts
            );

            var baseMonad = await base.GetStateMonad(newExternalContext, logger);

            return new StateMonad(
                baseMonad.Logger,
                baseMonad.StepFactoryStore,
                newExternalContext,
                baseMonad.SequenceMetadata
            );
        }
    }

    public record IntegrationTestCase : CaseThatExecutes
    {
        public IntegrationTestCase(
            string name,
            NuixRunScript step,
            string expectedOutput,
            params string[] expectedLoggedValues)
            : base(name, expectedLoggedValues)
        {
            Name = name;

            var sequence = new Sequence<StringStream>
            {
                InitialSteps = new List<IStep<Unit>>
                {
                    new SetVariable<StringStream>
                    {
                        Variable = new VariableName("Output"), Value = step
                    },
                    new NuixCloseConnection(),
                },
                FinalStep = GetVariable<StringStream>(new VariableName("Output"))
            };

            Step             = sequence;
            IgnoreFinalState = true;
            MyExpectedOutput = expectedOutput;

            var connectorInjections = new IConnectorInjection[] { new ConnectorInjection() };

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

        public Sequence<StringStream> Step { get; }

        public string MyExpectedOutput { get; }

        /// <inheritdoc />
        public override async Task<IStep> GetStepAsync(
            IExternalContext externalContext,
            ITestOutputHelper testOutputHelper)
        {
            await Task.CompletedTask;
            var yaml = Step.Serialize();

            var sfs = SettingsHelpers.CreateStepFactoryStore(null);

            var deserializedStep = SCLParsing.TryParseStep(yaml);

            deserializedStep.ShouldBeSuccessful();

            var unfrozenStep = deserializedStep.Value.TryFreeze(SCLRunner.RootCallerMetadata, sfs);

            unfrozenStep.ShouldBeSuccessful();

            return unfrozenStep.Value;
        }

        /// <inheritdoc />
        public override void CheckUnitResult(Result<Unit, IError> result)
        {
            throw new NotImplementedException("Expecting an output result");
        }

        /// <inheritdoc />
        public override void CheckOutputResult(Result<StringStream, IError> result)
        {
            result.ShouldBeSuccessful();

            result.Value.Should().Be(MyExpectedOutput);
        }

        /// <inheritdoc />
        public override async Task<StateMonad> GetStateMonad(
            IExternalContext externalContext,
            ILogger logger)
        {
            var baseMonad = await base.GetStateMonad(externalContext, logger);

            return new StateMonad(
                baseMonad.Logger,
                baseMonad.StepFactoryStore,
                externalContext,
                baseMonad.SequenceMetadata
            );
        }
    }
}
