using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Moq;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;
using Entity = Reductech.EDR.Core.Entities.Entity;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixRunScriptTests : StepTestBase<NuixRunScript, string>
    {
        /// <inheritdoc />
        public NuixRunScriptTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new RunScriptStepCase("Run Script, no entity Stream",
                    new NuixRunScript
                    {
                        FunctionName= Constant("Test Script"),
                        ScriptText = Constant("Lorem Ipsum"),
                        EntityStreamParameter = null,
                        Parameters = new Constant<Entity>(CreateEntity(("param1", "ABC")))

                    },
                    "Hello World",
                    new List<ExternalProcessAction>()
                    {
                        new ExternalProcessAction(new ConnectionCommand()
                        {
                            Command = "test_Script",
                            FunctionDefinition = "Lorem Ipsum",
                            Arguments = new Dictionary<string, object>()
                            {
                                {"param1","ABC"}
                            },IsStream = null
                        }, new ConnectionOutput()
                        {
                            Log = new ConnectionOutputLog(){Message = "Log Message", Severity = "info"},
                            Result = new ConnectionOutputResult(){Data = "Hello World"}
                        })
                    },
                    "Log Message"
                ).WithSettings(UnitTestSettings);

                yield return new RunScriptStepCase("Run Script with entity Stream",
                    new NuixRunScript
                    {
                        FunctionName = Constant("test_Script"),
                        ScriptText = Constant("Lorem Ipsum"),
                        EntityStreamParameter = new Constant<EntityStream>(new EntityStream(new List<Entity>()
                        {
                            CreateEntity(("Foo", "a")),
                            CreateEntity(("Foo", "b")),
                        }.ToAsyncEnumerable())),
                        Parameters = new Constant<Entity>(CreateEntity(("param1", "ABC")))

                    },
                    "Hello World",
                    new List<ExternalProcessAction>()
                    {
                        new ExternalProcessAction(new ConnectionCommand()
                        {
                            Command = "test_Script",
                            FunctionDefinition = "Lorem Ipsum",
                            Arguments = new Dictionary<string, object>()
                            {
                                {"param1","ABC"}
                            },IsStream = true
                        }, new ConnectionOutput()
                        {
                            Log = new ConnectionOutputLog(){Message = "Log Message", Severity = "info"},
                            Result = new ConnectionOutputResult(){Data = "Hello World"}
                        })
                    },
                    "Log Message"
                ).WithSettings(UnitTestSettings);

            }
        }

        public const string TestNuixPath = "TestPath";

        public NuixSettings UnitTestSettings => new NuixSettings(true, TestNuixPath, new Version(8,2),new List<NuixFeature>());

        /// <inheritdoc />
        protected override IEnumerable<SerializeCase> SerializeCases
        {
            get

            {
                var step = CreateStepWithDefaultOrArbitraryValues();
                yield return new SerializeCase("Default", step.step,
                    @"Do: NuixRunScript
FunctionName: 'Bar6'
ScriptText: 'Bar9'
Parameters: (Prop1 = 'Val7',Prop2 = 'Val8')
EntityStreamParameter:
- (Prop1 = 'Val0',Prop2 = 'Val1')
- (Prop1 = 'Val2',Prop2 = 'Val3')
- (Prop1 = 'Val4',Prop2 = 'Val5')"

                    );

            }
        }

        [Fact]

        [Trait("Category", "Integration")]
        public async Task TestScriptWithStream_Integration()
        {
            var stepCase = new IntegrationTestCase("Case with stream",
                new NuixRunScript
                {
                    FunctionName = Constant("test_Script2"),
                    ScriptText = Constant("log param1\r\nlog datastream.pop\r\nlog datastream.pop\r\nreturn param2"),
                    EntityStreamParameter = new Constant<EntityStream>(new EntityStream(new List<Entity>()
                        {
                            CreateEntity(("Foo", "a")),
                            CreateEntity(("Foo", "b")),
                        }.ToAsyncEnumerable())),
                    Parameters = new Constant<Entity>(CreateEntity(("param1", "ABC"), ("param2", "DEF")))

                },
                "DEF",
                "Starting",
                "ABC",
                "{\"Foo\":\"a\"}",
                "{\"Foo\":\"b\"}"
                
                ).WithSettings(Constants.NuixSettingsList.OrderByDescending(x=>x.NuixVersion).First());

            await stepCase.RunCaseAsync(TestOutputHelper, null);
        }

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestScriptWithNoStream_Integration()
        {
            var stepCase = new IntegrationTestCase("Case without stream",
                new NuixRunScript
                {
                    FunctionName = Constant("test_Script"),
                    ScriptText = Constant("log param1\r\nreturn param2"),
                    EntityStreamParameter = null,
                    Parameters = new Constant<Entity>(CreateEntity(("param1", "ABC"), ("param2", "DEF")))

                },
                "DEF",
                "Starting",
                "ABC").WithSettings(Constants.NuixSettingsList.OrderByDescending(x=>x.NuixVersion).First());;

            await stepCase.RunCaseAsync(TestOutputHelper, null);
        }


        public class RunScriptStepCase : StepCase
        {


            public RunScriptStepCase(string name,
                NuixRunScript step,
                string expectedOutput,
                IReadOnlyCollection<ExternalProcessAction> externalProcessActions, params string[] expectedLogValues)
                : base(name, step, expectedOutput, expectedLogValues)
            {
                ExternalProcessActions = externalProcessActions;
                IgnoreFinalState = true;
            }

            public IReadOnlyCollection<ExternalProcessAction> ExternalProcessActions { get; }

            /// <inheritdoc />
            public override IExternalProcessRunner GetExternalProcessRunner(MockRepository mockRepository) =>
                new ExternalProcessMock(1, ExternalProcessActions.ToArray());
        }

        public class IntegrationTestCase : CaseThatExecutes
        {
            public IntegrationTestCase(string name, NuixRunScript step, string expectedOutput, params string[] expectedLoggedValues)
                : base(expectedLoggedValues.Append(expectedOutput).ToArray())
            {
                Name = name;

                var sequence = new Sequence()
                {
                    Steps = new List<IStep<Unit>>()
                    {
                        new SetVariable<string>()
                        {
                            Variable = new VariableName("Output"),
                            Value = step
                        },
                        new NuixCloseConnection(),
                        new Print<string>()
                        {
                            Value =GetVariable<string>(new VariableName("Output"))
                        }
                    }

                };

                Step = sequence;
                IgnoreFinalState = true;
            }

            public override string Name { get; }

            public Sequence Step { get; }

            /// <inheritdoc />
            public override async Task<IStep> GetStepAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
            {
                var yaml = await Step.Unfreeze().SerializeToYamlAsync(CancellationToken.None);

                var deserializedStep = YamlMethods.DeserializeFromYaml(yaml, StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(NuixRunScript)));

                deserializedStep.ShouldBeSuccessful(x => x.AsString);

                var unfrozenStep = deserializedStep.Value.TryFreeze();

                unfrozenStep.ShouldBeSuccessful(x => x.AsString);

                return unfrozenStep.Value;
            }

            /// <inheritdoc />
            public override void CheckUnitResult(Result<Unit, IError> result)
            {
                result.ShouldBeSuccessful(x => x.AsString);
            }

            /// <inheritdoc />
            public override void CheckOutputResult(Result<string, IError> result)
            {
                result.ShouldBeSuccessful(x => x.AsString);

                (result.Value as object) .Should().Be(Unit.Default);
            }


            /// <inheritdoc />
            public override IFileSystemHelper GetFileSystemHelper(MockRepository mockRepository) => FileSystemHelper.Instance;

            /// <inheritdoc />
            public override IExternalProcessRunner GetExternalProcessRunner(MockRepository mockRepository) => ExternalProcessRunner.Instance;
        }


    }
}