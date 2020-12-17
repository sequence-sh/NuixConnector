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
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;
using Entity = Reductech.EDR.Core.Entity;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    [Collection("RequiresNuixLicense")]
    public class NuixRunScriptTests : StepTestBase<NuixRunScript, StringStream>
    {
        /// <inheritdoc />
        public NuixRunScriptTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                foreach (var errorCase in base.ErrorCases)
                {
                    yield return errorCase.WithSettings(UnitTestSettings);
                }
            }
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new RunScriptStepCase("Run Script, no entity Stream",
                    new NuixRunScript
                    {
                        FunctionName = Constant("Test Script"),
                        ScriptText = Constant("Lorem Ipsum"),
                        EntityStreamParameter = null,
                        Parameters = Constant(CreateEntity(("param1", "ABC")))
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
                            }
                        },
                        new ConnectionOutput()
                        {
                            Log = new ConnectionOutputLog() { Message = "Log Message", Severity = "info" }
                        },
                        new ConnectionOutput()
                        {
                            Result = new ConnectionOutputResult() { Data = "Hello World" }
                        })
                    },
                    "Log Message"
                ).WithSettings(UnitTestSettings)
                .WithFileSystemAction(x => x.Setup(f => f.DoesFileExist(It.IsAny<string>())).Returns(true));

                yield return new RunScriptStepCase("Run Script with entity Stream",
                    new NuixRunScript
                    {
                        FunctionName = Constant("test_Script"),
                        ScriptText = Constant("Lorem Ipsum"),
                        EntityStreamParameter = Constant(new EntityStream(new List<Entity>()
                        {
                            CreateEntity(("Foo", "a")),
                            CreateEntity(("Foo", "b")),
                        }.ToAsyncEnumerable())),
                        Parameters = Constant(CreateEntity(("param1", "ABC")))
                    },
                    @"[{""Foo"":""a""},{""Foo"":""b""}]",
                    new List<ExternalProcessAction>()
                    {
                        new ExternalProcessAction(new ConnectionCommand()
                        {
                            Command = "test_Script",
                            FunctionDefinition = "Lorem Ipsum",
                            Arguments = new Dictionary<string, object>()
                            {
                                {"param1","ABC"}
                            },
                            IsStream = true
                        }, new ConnectionOutput()
                        {
                            Log = new ConnectionOutputLog(){Message = "Log Message", Severity = "info"}
                        })
                    },
                    "Log Message"
                ).WithSettings(UnitTestSettings)
                .WithFileSystemAction(x => x.Setup(f => f.DoesFileExist(It.IsAny<string>())).Returns(true));
            }
        }

        public const string TestNuixPath = "TestPath";

        public static  NuixSettings UnitTestSettings => new NuixSettings(true, TestNuixPath, new Version(8, 2), new List<NuixFeature>());

        [Fact]
        [Trait("Category", "Integration")]
        public async Task TestScriptWithStream_Integration()
        {
            var stepCase = new IntegrationTestCase("Case with stream",
                new NuixRunScript
                {
                    FunctionName = Constant("test_Script2"),
                    ScriptText = Constant("log param1\r\nlog datastream.pop\r\nlog datastream.pop\r\nreturn param2"),
                    EntityStreamParameter = Constant(new EntityStream(new List<Entity>()
                        {
                            CreateEntity(("Foo", "a")),
                            CreateEntity(("Foo", "b")),
                        }.ToAsyncEnumerable())),
                    Parameters = Constant(CreateEntity(("param1", "ABC"), ("param2", "DEF")))

                },
                "DEF",
                "Starting",
                "ABC",
                "{\"Foo\":\"a\"}",
                "{\"Foo\":\"b\"}",
                "Finished"
            ).WithSettings(Constants.NuixSettingsList.OrderByDescending(x => x.NuixVersion).First());

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
                    Parameters =  Constant(CreateEntity(("param1", "ABC"), ("param2", "DEF")))

                },
                "DEF",
                "Starting",
                "ABC",
                "Finished"
            ).WithSettings(Constants.NuixSettingsList.OrderByDescending(x => x.NuixVersion).First());
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
                : base(expectedLoggedValues)
            {
                Name = name;

                var sequence = new Sequence<StringStream>()
                {
                    InitialSteps = new List<IStep<Unit>>()
                    {
                        new SetVariable<StringStream>()
                        {
                            Variable = new VariableName("Output"),
                            Value = step
                        },
                        new NuixCloseConnection(),
                    },
                    FinalStep = GetVariable<StringStream>(new VariableName("Output"))

                };

                Step = sequence;
                IgnoreFinalState = true;
                ExpectedOutput = expectedOutput;
            }

            public override string Name { get; }

            public Sequence<StringStream> Step { get; }

            public string ExpectedOutput { get; }

            /// <inheritdoc />
            public override async Task<IStep> GetStepAsync(ITestOutputHelper testOutputHelper, string? extraArgument)
            {
                var yaml = await Step.SerializeAsync(CancellationToken.None);

                var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(NuixRunScript));


                var deserializedStep = SequenceParsing.ParseSequence(yaml);

                deserializedStep.ShouldBeSuccessful(x => x.AsString);

                var unfrozenStep = deserializedStep.Value.TryFreeze(sfs);

                unfrozenStep.ShouldBeSuccessful(x => x.AsString);

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
                result.ShouldBeSuccessful(x => x.AsString);

                result.Value.Should().Be(ExpectedOutput);
            }


            /// <inheritdoc />
            public override IFileSystemHelper GetFileSystemHelper(MockRepository mockRepository) => FileSystemHelper.Instance;

            /// <inheritdoc />
            public override IExternalProcessRunner GetExternalProcessRunner(MockRepository mockRepository) => ExternalProcessRunner.Instance;
        }


    }
}