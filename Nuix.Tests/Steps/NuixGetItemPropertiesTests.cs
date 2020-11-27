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
                    new FileWrite
                    {
                        Path = new PathCombine{Paths = new Constant<List<string>>(new List<string>(){OutputFolder,"ItemProperties.txt"})},
                        Stream = new StringToStream
                        {
                            String = new NuixGetItemProperties
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