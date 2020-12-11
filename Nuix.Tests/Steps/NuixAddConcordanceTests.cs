using System.Collections.Generic;
using Moq;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixAddConcordanceTests : NuixStepTestBase<NuixAddConcordance, Unit>
    {
        /// <inheritdoc />
        public NuixAddConcordanceTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases
        {
            get
            {
                yield return new NuixStepCase("Add Concordance Test",
                    new NuixAddConcordance
                    {
                        ConcordanceProfileName = Constant("IntegrationTestProfile"),
                        ConcordanceDateFormat = Constant("yyyy-MM-dd'T'HH:mm:ss.SSSZ"),
                        FilePath = ConcordancePath,
                        Custodian = Constant("Mark"),
                        FolderName = Constant("New Folder"),
                        CasePath = CasePath,
                    },
                    Unit.Default,
                    new List<ExternalProcessAction>
                    {
                        new ExternalProcessAction(
                            new ConnectionCommand
                            {
                                Command = "AddConcordanceToCase",
                                FunctionDefinition = "",
                                Arguments = new Dictionary<string, object>
                                {
                        {nameof(NuixAddConcordance.CasePath), CasePathString},
                        {nameof(NuixAddConcordance.FolderName), "New Folder"},
                        {nameof(NuixAddConcordance.Custodian), "Mark"},
                        {nameof(NuixAddConcordance.FilePath), ConcordancePathString},
                        {nameof(NuixAddConcordance.ConcordanceDateFormat), "yyyy-MM-dd'T'HH:mm:ss.SSSZ"},
                        {nameof(NuixAddConcordance.ConcordanceProfileName), "IntegrationTestProfile"}
                                }

                            },
                            new ConnectionOutput
                            {
                                Result = new ConnectionOutputResult(){Data = null}
                            }
                        )
                    }).WithSettings(UnitTestSettings);

            }
        }


        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get
            {
                string concordanceDateFormat = "yyyy-MM-dd''T''HH:mm:ss.SSSZ";

                var integrationTestProfile = @"IntegrationTestProfile";
                var custodian = @"Mark";
                var newFolder = @"New Folder";
                yield return new NuixDeserializeTest("Add Concordance",
                        $@"NuixAddConcordance CasePath: '{CasePathString}' ConcordanceDateFormat: '{concordanceDateFormat}' ConcordanceProfileName: '{integrationTestProfile}' Custodian: '{custodian}' FilePath: '{ConcordancePathString}' FolderName: '{newFolder}'",
                        Unit.Default,
                        new List<ExternalProcessAction>
                        {
                            new ExternalProcessAction(new ConnectionCommand
                            {
                                Command = "AddConcordanceToCase",
                                Arguments = new Dictionary<string, object>
                                {
                                    {nameof(NuixAddConcordance.CasePath), CasePathString},
                                    {nameof(NuixAddConcordance.FolderName), newFolder},
                                    {nameof(NuixAddConcordance.Custodian), custodian},
                                    {nameof(NuixAddConcordance.FilePath), ConcordancePathString},
                                    {
                                        nameof(NuixAddConcordance.ConcordanceDateFormat),
                                        concordanceDateFormat.Replace("''", "'")
                                    },
                                    {nameof(NuixAddConcordance.ConcordanceProfileName), integrationTestProfile}
                                }
                            }, new ConnectionOutput {Result = new ConnectionOutputResult {Data = null}})
                        }
                    ).WithSettings(UnitTestSettings)
                    .WithFileSystemAction(x => x.Setup(f => f.DoesFileExist(It.IsAny<string>())).Returns(true));
            }
        }


        /// <inheritdoc />
        protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
        {
            get
            {
                yield return new NuixIntegrationTestCase("Add concordance to case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new NuixAddConcordance
                    {
                        ConcordanceProfileName = Constant("IntegrationTestProfile"),
                        ConcordanceDateFormat = Constant("yyyy-MM-dd'T'HH:mm:ss.SSSZ"),
                        FilePath = ConcordancePath,
                        Custodian = Constant("Mark"),
                        FolderName = Constant("New Folder"),
                        CasePath = CasePath
                    },
                    AssertCount(1, "*.txt"),
                    DeleteCaseFolder
                );

            }
        }
    }
}
