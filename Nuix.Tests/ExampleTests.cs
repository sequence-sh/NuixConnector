using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Internal.Parser;
using Reductech.EDR.Core.Internal.Serialization;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Entity = Reductech.EDR.Core.Entity;

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

        var stepResult = SCLParsing.ParseSequence(yaml)
            .Bind(x => x.TryFreeze(sfs));
        //.Map(SCLRunner.ConvertToUnitStep);

        if (stepResult.IsFailure)
            errorAction.Invoke(stepResult);

        var settings = CreateSettings();

        var monad = new StateMonad(
            logger,
            settings,
            sfs,
            ExternalContext.Default
        );

        var r = await stepResult.Value.Run<Unit>(monad, CancellationToken.None);

        r.ShouldBeSuccessful(x => x.AsString);

        static SCLSettings CreateSettings()
        {
            var dict = new Dictionary<string, object>
            {
                {
                    NuixSettings.NuixSettingsKey, new Dictionary<string, object>
                    {
                        {
                            NuixSettings.ConsolePathKey, Path.Combine(
                                @"C:\Program Files\Nuix\Nuix 8.8",
                                Constants.NuixConsoleExe
                            )
                        },
                        { SCLSettings.VersionKey, new Version(8, 8).ToString() },
                        { NuixSettings.LicenceSourceTypeKey, "server" },
                        { NuixSettings.LicenceSourceLocationKey, "license location" },
                        //{ NuixSettings.LicenceTypeKey, "enterprise-workstation" },
                        {
                            NuixSettings.ConsoleArgumentsKey,
                            new List<string>()
                            {
                                "-Dnuix.licence.handlers=server",
                                "-Dnuix.registry.servers=license server",
                            }
                        },
                        {
                            NuixSettings.EnvironmentVariablesKey,
                            new Dictionary<string, string>()
                            {
                                { "NUIX_USERNAME", "user" }, { "NUIX_PASSWORD", "password" },
                            }
                        },
                        {
                            SCLSettings.FeaturesKey, Enum.GetNames(typeof(NuixFeature))
                                .Select(x => x.ToString())
                                .ToList()
                        }
                    }
                }
            };

            var entity = Entity.Create((SCLSettings.ConnectorsKey, dict));

            return new SCLSettings(entity);
        }
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
                    result.Error.GetAllErrors().Select(x => x.Message + " " + x.Location.AsString())
                )
            )
        );
    }

    [Fact(Skip = "manual")]
    //[Fact]
    public async Task RunYamlSequence()
    {
        const string yaml = @"NuixDoesCaseExist 'abc'";

        await RunYamlSequenceInternal(
            yaml,
            result =>
                result.ShouldBeSuccessful(x => x.AsString)
        );
    }
}

}
