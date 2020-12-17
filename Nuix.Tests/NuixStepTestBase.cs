using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.TestHarness;
using Xunit.Abstractions;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    //TODO test script composition
    //TODO test deserialization

    public abstract partial class NuixStepTestBase<TStep, TOutput> : StepTestBase<TStep, TOutput>
        where TStep : class, IRubyScriptStep<TOutput>, new()
    {
        /// <inheritdoc />
        protected NuixStepTestBase([NotNull] ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
        }

        public const string TestNuixPath = "TestPath";

        private static bool IsVersionCompatible(IStep step, Version nuixVersion)
        {
            var features = Enum.GetValues(typeof(NuixFeature)).Cast<NuixFeature>().ToHashSet();
            var settings = new NuixSettings(false, "", nuixVersion, features);
            var r = step.Verify(settings);
            return r.IsSuccess;
        }


        public NuixSettings UnitTestSettings
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
                foreach (var baseErrorCase in base.ErrorCases)
                {
                    var caseWithSettings = baseErrorCase.WithSettings(UnitTestSettings);
                    yield return caseWithSettings;
                }
            }
        }


        protected abstract IEnumerable<NuixIntegrationTestCase> NuixTestCases { get; }
    }
}