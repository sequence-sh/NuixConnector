using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Xunit;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Serialization;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public class FullWorkflowTests : FullWorkflowTestCases
    {
        public FullWorkflowTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory(Skip = "This test creates the case but doesn't delete it so it can only be run once")]
        [Trait("Category", "Integration")]
        [ClassData(typeof(FullWorkflowTestCases))]
        public override Task Test(string key) => base.Test(key);
    }

    public class FullWorkflowTestCases : TestBaseParallel
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCaseParallel> TestCases
        {
            get
            {
                var text = @"
- <CaseName> = 'EvidenceLoop'
- <CasePath> = 'D:/Antony/nuix-demo-2/case-ev3'
- <Investigator> = 'Antony'
- <EvidenceDir> = 'D:/Antony/nuix-demo-2'
- <EvidenceCsv> = 'evidence.csv'
- <ImportProcessingProfileName> = 'Default'


# Import evidence from CSV file
- <CsvHeader> = ['Custodian', 'FolderName', 'Path']
- <Bags> = ReadCsv(Text = ReadFile(Folder = <EvidenceDir>, FileName = <EvidenceCsv>), ColumnsToMap = <CsvHeader>)
- Do: ForEach
  Array: <Bags>
  VariableName: <Row>
  Action:
    Do: NuixAddItem
    CasePath: <CasePath>
    Custodian: ElementAtIndex(Array = <Row>, Index = 0)
    FolderName: ElementAtIndex(Array = <Row>, Index = 1)
    Path: ElementAtIndex(Array = <Row>, Index = 2)
";

                yield return new FullWorkflowTest("Create and import", text);

            }
        }

        private class FullWorkflowTest : ITestBaseCaseParallel
        {
            public FullWorkflowTest(string name, string yamlText)
            {
                Name = name;
                YamlText = yamlText;
            }

            /// <inheritdoc />
            public string Name { get; }

            public string YamlText { get; }


            /// <inheritdoc />
            public async Task ExecuteAsync(ITestOutputHelper testOutputHelper)
            {
                var loggerFactory = new LoggerFactory(new[] { new XunitLoggerProvider(testOutputHelper) });

                var logger = loggerFactory.CreateLogger(Name);
                var factoryStore = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(IRubyScriptStep));

                var settings = NuixTestCases.NuixSettingsList.OrderByDescending(x => x.NuixVersion).First();

                var stateMonad = new StateMonad(logger, settings, ExternalProcessRunner.Instance, factoryStore);


                var pfs = StepFactoryStore.CreateUsingReflection(typeof(RubyScriptStepUnit),
                    typeof(CompoundFreezableStep));

                var deserializeResult = YamlMethods.DeserializeFromYaml(YamlText, pfs);

                deserializeResult.ShouldBeSuccessful(x=>x.AsString);


                var freezeResult = deserializeResult.Value.TryFreeze().Bind(YamlRunner.ConvertToUnitStep);

                freezeResult.ShouldBeSuccessful(x=>x.AsString);


                var runResult = await freezeResult.Value.Run(stateMonad, CancellationToken.None);

                runResult.ShouldBeSuccessful(x=>x.AsString);
            }
        }
    }
}