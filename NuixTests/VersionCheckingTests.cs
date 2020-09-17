using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Xunit;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    [Collection("RequiresNuixLicense")]
    public class VersionCheckingTests
    {
        [Fact]
        [Trait(NuixTestCases.Category, NuixTestCases.Integration)]
        public void TestVersionCheckingWithinScript()
        {
            var baseSettings = NuixTestCases.NuixSettingsList.OrderByDescending(x => x.NuixVersion).FirstOrDefault();

            var superSettings = new NuixProcessSettings(baseSettings.UseDongle, baseSettings.NuixExeConsolePath, new Version(100, 0), baseSettings.NuixFeatures);

            baseSettings.Should().NotBeNull();

            var process = new DoNothing { MyRequiredVersion = new Version(100, 0) };

            var result = process.Run(new ProcessState(NullLogger.Instance, superSettings, ExternalProcessRunner.Instance));

            result.ShouldBeFailure(x => x.AsString, "Nuix Version is");
        }


        internal class DoNothing : RubyScriptProcessUnit
        {
            /// <inheritdoc />
            public override IRubyScriptProcessFactory<Unit> RubyScriptProcessFactory => new DoNothingProcessFactory(MyRequiredVersion, MyRequiredFeatures);

            public Version? MyRequiredVersion { get; set; }

            public List<NuixFeature>? MyRequiredFeatures { get; set; }

            internal class DoNothingProcessFactory : RubyScriptProcessFactory<DoNothing, Unit>
            {
                public DoNothingProcessFactory(Version? myRequiredVersion, List<NuixFeature>? myRequiredFeatures)
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
                public override string FunctionName => "DoNothing";

                /// <inheritdoc />
                public override string RubyFunctionText => @"
puts 'Doing Nothing'
";
            }
        }
    }
}