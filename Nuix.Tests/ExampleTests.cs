﻿using System;
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
using Reductech.EDR.Core.Serialization;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
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

        private readonly NuixSettings _nuixSettings = new NuixSettings(true,
            Constants.NuixSettingsList.First().NuixExeConsolePath,
            new Version(8, 8),
            Constants.AllNuixFeatures);

        private async Task RunYamlSequenceInternal(string yaml, Action<Result<IStep, IError>> errorAction)
        {
            var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(IRubyScriptStep));

            var stepResult = YamlMethods.DeserializeFromYaml(yaml, sfs).Bind(x => x.TryFreeze());

            if (stepResult.IsFailure)
                errorAction.Invoke(stepResult);

            var monad = new StateMonad(new TestLogger(), _nuixSettings,
                ExternalProcessRunner.Instance, FileSystemHelper.Instance, sfs);

            var r = await stepResult.Value.Run<Unit>(monad, CancellationToken.None);

            r.ShouldBeSuccessful(x => x.AsString);
        }

        [Theory(Skip = "Manual")]
        [InlineData(@"D:\temp\ExampleSequences\test.yml")]
        public async Task RunYamlSequenceFromFile(string path)
        {
            var yaml = await File.ReadAllTextAsync(path);

            TestOutputHelper.WriteLine(yaml);

            await RunYamlSequenceInternal(yaml, result => throw new XunitException(
                string.Join(", ",
                    result.Error.GetAllErrors().Select(x => x.Message + " " + x.Location.AsString))
            ));
        }

        [Fact(Skip = "manual")]
        public async Task RunYamlSequence()
        {
            const string yaml = @"";

            await RunYamlSequenceInternal(yaml, result =>
                result.ShouldBeSuccessful(x => x.AsString));
        }
    }
}
