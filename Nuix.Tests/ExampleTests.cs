using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging.Xunit;
using Reductech.Sequence.Core.Abstractions;
using Reductech.Sequence.Core.Internal.Errors;
using Reductech.Sequence.Core.Internal.Parser;
using Reductech.Sequence.Core.Internal.Serialization;
using Reductech.Sequence.Core.TestHarness;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Entity = Reductech.Sequence.Core.Entity;

namespace Reductech.Sequence.Connectors.Nuix.Tests;

/// <summary>
/// These are not really tests but ways to quickly and easily run steps
/// </summary>
public class ExampleTests
{
    public ExampleTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

    public ITestOutputHelper TestOutputHelper { get; }

    private async Task RunYamlSequenceInternal(
        string yaml,
        Action<Result<IStep, IError>> errorAction)
    {
        var sfs = CreateStepFactoryStore();

        var logger = new XunitLogger(TestOutputHelper, "Test");

        var stepResult = SCLParsing.TryParseStep(yaml)
            .Bind(x => x.TryFreeze(SCLRunner.RootCallerMetadata, sfs));

        if (stepResult.IsFailure)
            errorAction.Invoke(stepResult);

        var monad = new StateMonad(
            logger,
            sfs,
            ExternalContext.Default,
            new Dictionary<string, object>()
        );

        var r = await stepResult.Value.Run<Unit>(monad, CancellationToken.None);

        r.ShouldBeSuccessful();

        static StepFactoryStore CreateStepFactoryStore()
        {
            return SettingsHelpers.CreateStepFactoryStore(
                new NuixSettings(
                    Path.Combine(
                        @"C:\Program Files\Nuix\Nuix 8.8",
                        Constants.NuixConsoleExe
                    ),
                    new Version(8, 8),
                    false,
                    Enum.GetValues<NuixFeature>()
                )
                {
                    LicenceSourceType     = "server",
                    LicenseSourceLocation = "license location",
                    ConsoleArguments = new List<string>()
                    {
                        "-Dnuix.licence.handlers=server",
                        "-Dnuix.registry.servers=license server",
                    },
                    EnvironmentVariables = Entity.Create(
                        ("NUIX_USERNAME", "user"),
                        ("NUIX_PASSWORD", "password")
                    )
                }
            );
        }
    }

    #pragma warning disable xUnit1004 // Test methods should not be skipped
    [Theory(Skip = "Manual")]
    #pragma warning restore xUnit1004 // Test methods should not be skipped
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
                    result.Error.GetAllErrors().Select(x => x.Message + " " + x.Location.AsString())
                )
            )
        );
    }

    #pragma warning disable xUnit1004 // Test methods should not be skipped
    [Fact(Skip = "manual")]
    #pragma warning restore xUnit1004 // Test methods should not be skipped
    //[Fact]
    public async Task RunYamlSequence()
    {
        const string yaml = @"NuixDoesCaseExist 'abc'";

        await RunYamlSequenceInternal(
            yaml,
            result =>
                result.ShouldBeSuccessful()
        );
    }
}
