using System.Collections.Generic;
using Moq;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class NuixAddConcordanceTests : NuixStepTestBase<NuixAddConcordance, Unit>
{
    /// <inheritdoc />
    protected override IEnumerable<StepCase> StepCases
    {
        get
        {
            yield return new NuixStepCase(
                "Add Concordance Test",
                new NuixAddConcordance
                {
                    ConcordanceProfileName = Constant("IntegrationTestProfile"),
                    ConcordanceDateFormat  = Constant("yyyy-MM-dd'T'HH:mm:ss.SSSZ"),
                    FilePath               = ConcordancePath,
                    Custodian              = Constant("Mark"),
                    Container              = Constant("New Folder"),
                    CasePath               = CasePath,
                },
                Unit.Default,
                new List<ExternalProcessAction>
                {
                    new(
                        new ConnectionCommand
                        {
                            Command            = "AddConcordanceToCase",
                            FunctionDefinition = "",
                            Arguments = new Dictionary<string, object>
                            {
                                { nameof(NuixAddConcordance.CasePath), CasePathString },
                                { nameof(NuixAddConcordance.Container), "New Folder" },
                                { nameof(NuixAddConcordance.Custodian), "Mark" },
                                {
                                    nameof(NuixAddConcordance.FilePath),
                                    ConcordancePathString
                                },
                                {
                                    nameof(NuixAddConcordance.ConcordanceDateFormat),
                                    "yyyy-MM-dd'T'HH:mm:ss.SSSZ"
                                },
                                {
                                    nameof(NuixAddConcordance.ConcordanceProfileName),
                                    "IntegrationTestProfile"
                                }
                            }
                        },
                        new ConnectionOutput
                        {
                            Result = new ConnectionOutputResult() { Data = null }
                        }
                    )
                }
            ).WithSettings(UnitTestSettings);
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<DeserializeCase> DeserializeCases
    {
        get
        {
            string concordanceDateFormat = "yyyy-MM-dd''T''HH:mm:ss.SSSZ";

            var integrationTestProfile = @"IntegrationTestProfile";
            var custodian              = @"Mark";
            var newFolder              = @"New Folder";

            yield return new NuixDeserializeTest(
                    "Add Concordance",
                    $@"NuixAddConcordance CasePath: '{CasePathString}' ConcordanceDateFormat: '{concordanceDateFormat}' ConcordanceProfileName: '{integrationTestProfile}' Custodian: '{custodian}' FilePath: '{ConcordancePathString}' FolderName: '{newFolder}'",
                    Unit.Default,
                    new List<ExternalProcessAction>
                    {
                        new(
                            new ConnectionCommand
                            {
                                Command = "AddConcordanceToCase",
                                Arguments = new Dictionary<string, object>
                                {
                                    { nameof(NuixAddConcordance.CasePath), CasePathString },
                                    { nameof(NuixAddConcordance.Container), newFolder },
                                    { nameof(NuixAddConcordance.Custodian), custodian },
                                    {
                                        nameof(NuixAddConcordance.FilePath),
                                        ConcordancePathString
                                    },
                                    {
                                        nameof(NuixAddConcordance.ConcordanceDateFormat),
                                        concordanceDateFormat.Replace("''", "'")
                                    },
                                    {
                                        nameof(NuixAddConcordance.ConcordanceProfileName),
                                        integrationTestProfile
                                    }
                                }
                            },
                            new ConnectionOutput
                            {
                                Result = new ConnectionOutputResult { Data = null }
                            }
                        )
                    }
                ).WithSettings(UnitTestSettings)
                .WithFileAction(x => x.Setup(f => f.Exists(It.IsAny<string>())).Returns(true));
        }
    }

    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Add concordance to case",
                DeleteCaseFolder,
                CreateCase,
                new NuixAddConcordance
                {
                    FilePath               = ConcordancePath,
                    Container              = Constant("New Folder"),
                    ConcordanceProfileName = Constant("IntegrationTestProfile"),
                    ConcordanceDateFormat  = Constant("yyyy-MM-dd'T'HH:mm:ss.SSSZ"),
                    Custodian              = Constant("Mark"),
                    Description            = Constant("Container description"),
                    ContainerEncoding      = Constant("UTF-8"),
                    ContainerLocale        = Constant("en_GB"),
                    ContainerTimeZone      = Constant("UTC")
                },
                AssertCount(2, "*.txt"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );

            yield return new NuixIntegrationTestCase(
                "Add concordance to case with custom ProcessingSettings",
                DeleteCaseFolder,
                CreateCase,
                new NuixAddConcordance
                {
                    FilePath               = ConcordancePath,
                    Container              = Constant("New Folder"),
                    ConcordanceProfileName = Constant("IntegrationTestProfile"),
                    ProcessingSettings     = Constant(Entity.Create(("create_thumbnails", false))),
                    CustomMetadata         = Constant(Entity.Create(("CustomMeta", "value")))
                },
                AssertCount(2, "*.txt"),
                AssertCount(2, "custom-metadata:\"CustomMeta\":*"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );

            yield return new NuixIntegrationTestCase(
                "Add concordance to case with opticon file",
                DeleteCaseFolder,
                CreateCase,
                new NuixAddConcordance
                {
                    FilePath               = ConcordancePath,
                    Container              = Constant("New Folder"),
                    ConcordanceProfileName = Constant("IntegrationTestProfile"),
                    OpticonPath            = OpticonPath
                },
                AssertCount(2, "*"),
                AssertCount(2, "*.txt"),
                new NuixCloseConnection(),
                DeleteCaseFolder
            );
        }
    }
}

}
