using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
using Xunit.Abstractions;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{
    public class NuixGetItemPropertiesTests : NuixStepTestBase<NuixGetItemProperties, string>
    {
        /// <inheritdoc />
        public NuixGetItemPropertiesTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }


        /// <inheritdoc />
        protected override IEnumerable<DeserializeCase> DeserializeCases
        {
            get { yield break; }

        }

        /// <inheritdoc />
        protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases {
            get
            {
                yield return new NuixIntegrationTestCase("Get Item Properties",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    AddData,
                    new WriteFile
                    {
                        FileName = new Constant<string>("ItemProperties.txt"),
                        Folder = Constant(OutputFolder),
                        Text = new ToStream
                        {
                            Text = new NuixGetItemProperties
                            {
                                CasePath = CasePath,
                                PropertyRegex = Constant("(.+)"),
                                SearchTerm = Constant("*")
                            }
                        }
                    },
                    AssertFileContains(OutputFolder, "ItemProperties.txt",
                        "Character Set	UTF-8	New Folder/data/Jellyfish.txt"),

                    DeleteCaseFolder,
                    DeleteOutputFolder
                );

            } }
    }
}