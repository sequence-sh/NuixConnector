using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

/// <summary>
/// These are not really tests but ways to quickly and easily run steps
/// </summary>
public class ExampleTests
{
    public ExampleTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

    public ITestOutputHelper TestOutputHelper { get; }

    private SCLSettings _nuixSettings => Constants.NuixSettingsList.First();

    private async Task RunYamlSequenceInternal(
        string yaml,
        Action<Result<IStep, IError>> errorAction)
    {
        var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(IRubyScriptStep));

        var logger = new Microsoft.Extensions.Logging.Xunit.XunitLogger(TestOutputHelper, "Test");

        var stepResult = SCLParsing.ParseSequence(yaml).Bind(x => x.TryFreeze(sfs));

        if (stepResult.IsFailure)
            errorAction.Invoke(stepResult);

        var monad = new StateMonad(
            logger,
            _nuixSettings,
            ExternalProcessRunner.Instance,
            FileSystemHelper.Instance,
            sfs
        );

        var r = await stepResult.Value.Run<Unit>(monad, CancellationToken.None);

        r.ShouldBeSuccessful(x => x.AsString);
    }

    [Theory(Skip = "Manual")]
    [InlineData(@"D:\temp\ExampleSequences\test.yml")]
    public async Task RunYamlSequenceFromFile(string path)
    {
        var yaml = await File.ReadAllTextAsync(path);

        TestOutputHelper.WriteLine(yaml);

        await RunYamlSequenceInternal(
            yaml,
            result => throw new XunitException(
                string.Join(
                    ", ",
                    result.Error.GetAllErrors().Select(x => x.Message + " " + x.Location.AsString)
                )
            )
        );
    }

    [Fact(Skip = "manual")]
    public async Task RunYamlSequence()
    {
        const string yaml = @"- <CurrentDir>   = 'D:\temp'
- <CasePath>     = PathCombine [<CurrentDir>, 'case']
- <SearchTagCSV> = PathCombine [<CurrentDir>, 'searchtag.csv']
- NuixOpenConnection
- ReadFile <SearchTagCSV>
  | FromCsv
  | EntityForEach
    Action: (
      NuixSearchAndTag
        CasePath: <CasePath>
        SearchTerm: (EntityGetValue <Entity> 'SearchTerm')
        Tag: (EntityGetValue <Entity> 'Tag')
    )";

        await RunYamlSequenceInternal(
            yaml,
            result =>
                result.ShouldBeSuccessful(x => x.AsString)
        );
    }
}

}
