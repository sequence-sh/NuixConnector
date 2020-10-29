using System.Collections.Generic;
using JetBrains.Annotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public abstract class NuixStepTestBase<TStep, TOutput> : StepTestBase<TStep,TOutput> where TStep : class, IRubyScriptStep<TOutput>, new()
    {
        /// <inheritdoc />
        protected NuixStepTestBase([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        public const string TestNuixPath = "TestPath";

        public NuixSettings Settings
        {
            get
            {
                var instance = new TStep();

                var factory = instance.RubyScriptStepFactory;

                return new NuixSettings(true, TestNuixPath, factory.RequiredNuixVersion, factory.RequiredFeatures);
            }
        }


        /// <inheritdoc />
        protected override IEnumerable<ErrorCase> ErrorCases
        {
            get
            {
                var defaultError = CreateDefaultErrorCase().WithSettings(Settings);

                yield return defaultError;

                //var previousVersion = RequiredVersion.Minor == 0
                //    ? new Version(RequiredVersion.Major - 1, 10)
                //    : new Version(RequiredVersion.Major, RequiredVersion.Minor - 1);

                //yield return new ErrorCase("Too low version", CreateStepWithFailStepsAsValues(),
                //    new ErrorBuilder("Version too low", ErrorCode.RequirementsNotMet))
                //        .WithSettings(new NuixSettings(Settings.UseDongle, TestNuixPath, previousVersion, Features));


                //foreach (var nuixFeature in Features)
                //{
                //    yield return new ErrorCase($"Missing Feature {nuixFeature}", CreateStepWithFailStepsAsValues(),
                //        new ErrorBuilder("Missing Feature", ErrorCode.RequirementsNotMet))
                //            .WithSettings(new NuixSettings(Settings.UseDongle, TestNuixPath, RequiredVersion, Features.Remove(nuixFeature))
                //        );
                //}

            }
        }
    }
}
