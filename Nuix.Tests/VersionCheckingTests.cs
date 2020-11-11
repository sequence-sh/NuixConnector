using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    [Collection("RequiresNuixLicense")]
    public class VersionCheckingTests
    {

        [Fact]
        [Trait(Constants.Category, Constants.Integration)]
        public async Task Nuix_script_should_fail_if_required_version_is_above_nuix_version()
        {

            var superSettings = new NuixSettings(true, Constants.NuixSettingsList.First().NuixExeConsolePath, new Version(100, 0), Constants.AllNuixFeatures);

            var factoryStore = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(IRubyScriptStep));
            var stateMonad = new StateMonad(NullLogger.Instance, superSettings, ExternalProcessRunner.Instance, FileSystemHelper.Instance, factoryStore);
            var process = new NuixDoNothing { MyRequiredVersion = new Version(100, 0) };

            var result = await process
                .Run(stateMonad, CancellationToken.None)
                .MapError(x => x.AsString);

            result.ShouldBeFailure();

            result.Error.Should().Contain("Nuix Version is");
        }


        internal class NuixDoNothing : RubyScriptStepUnit
        {
            /// <inheritdoc />
            public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => new DoNothingStepFactory(MyRequiredVersion, MyRequiredFeatures);

            public Version? MyRequiredVersion { get; set; }

            public List<NuixFeature>? MyRequiredFeatures { get; set; }

            internal class DoNothingStepFactory : RubyScriptStepFactory<NuixDoNothing, Unit>
            {
                public DoNothingStepFactory(Version? myRequiredVersion, List<NuixFeature>? myRequiredFeatures)
                {
                    MyRequiredVersion = myRequiredVersion;
                    MyRequiredFeatures = myRequiredFeatures;
                }



                /// <inheritdoc />
                public override Version RequiredNuixVersion => MyRequiredVersion ?? new Version(1, 0);

                public Version? MyRequiredVersion { get; }

                /// <inheritdoc />
                public override IReadOnlyCollection<NuixFeature> RequiredFeatures => MyRequiredFeatures ?? new List<NuixFeature>();

                public List<NuixFeature>? MyRequiredFeatures { get; }

                /// <inheritdoc />
                public override string FunctionName => "NuixDoNothing";

                /// <inheritdoc />
                public override string RubyFunctionText => @"
puts 'Doing Nothing'
";
            }
        }
    }
}