using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
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

            var superSettings = new NuixSettings(baseSettings.UseDongle, baseSettings.NuixExeConsolePath, new Version(100, 0), baseSettings.NuixFeatures);

            baseSettings.Should().NotBeNull();

            var process = new DoNothing { MyRequiredVersion = new Version(100, 0) };

            var result = process.Run(new StateMonad(NullLogger.Instance, superSettings, ExternalProcessRunner.Instance));

            result.MapFailure(x=>x.AsString).ShouldBeFailure("Nuix Version is");
        }


        internal class DoNothing : RubyScriptStepUnit
        {
            /// <inheritdoc />
            public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => new DoNothingStepFactory(MyRequiredVersion, MyRequiredFeatures);

            public Version? MyRequiredVersion { get; set; }

            public List<NuixFeature>? MyRequiredFeatures { get; set; }

            internal class DoNothingStepFactory : RubyScriptStepFactory<DoNothing, Unit>
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
                public override string FunctionName => "DoNothing";

                /// <inheritdoc />
                public override string RubyFunctionText => @"
puts 'Doing Nothing'
";
            }
        }
    }
}