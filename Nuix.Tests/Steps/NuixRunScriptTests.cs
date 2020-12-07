using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

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
                        Parameters = new Constant<Entity>(CreateEntity(("Param1", "ABC")))

                    },
                    "Hello World",
                    new List<ExternalProcessAction>()
                    {
                        new ExternalProcessAction(new ConnectionCommand()
                        {
                            Command = "Test Script",
                            FunctionDefinition = "Lorem Ipsum",
                            Arguments = new Dictionary<string, object>()
                            {
                                {"Param1","ABC"}
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
                        FunctionName = Constant("Test Script"),
                        ScriptText = Constant("Lorem Ipsum"),
                        EntityStreamParameter = new Constant<EntityStream>(new EntityStream(new List<Entity>()
                        {
                            CreateEntity(("Foo", "a")),
                            CreateEntity(("Foo", "b")),
                        }.ToAsyncEnumerable())),
                        Parameters = new Constant<Entity>(CreateEntity(("Param1", "ABC")))

                    },
                    "Hello World",
                    new List<ExternalProcessAction>()
                    {
                        new ExternalProcessAction(new ConnectionCommand()
                        {
                            Command = "Test Script",
                            FunctionDefinition = "Lorem Ipsum",
                            Arguments = new Dictionary<string, object>()
                            {
                                {"Param1","ABC"}
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


    }
}