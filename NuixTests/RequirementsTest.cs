using System;
using System.Collections.Generic;
using NUnit.Framework;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public class RequirementsTest
    {

        public static readonly IReadOnlyCollection<(string? expectedError, NuixProcessSettings settings)> TestCases =
            new List<(string? expectedError, NuixProcessSettings settings)>
            {
                ( "Required Nuix Version >= 5.0 but had 1.0",
                    new NuixProcessSettings(true, "abcd", new Version(1,0), new List<NuixFeature>{NuixFeature.ANALYSIS} ) ),

                ( "ANALYSIS missing",
                    new NuixProcessSettings(true, "abcd", new Version(8,0), new List<NuixFeature>() ) ),

                ("Required Nuix Version >= 5.0 but had 1.0; ANALYSIS missing",
                    new NuixProcessSettings(true, "abcd", new Version(1,0), new List<NuixFeature>() ) ),

                (null,new NuixProcessSettings(true, "abcd", new Version(8,0), new List<NuixFeature>{NuixFeature.ANALYSIS} ) )
            };

        [TestCaseSource(nameof(TestCases))]
        [Test]
        public void TestRequirements((string? expectedError, NuixProcessSettings settings) args)
        {
            var process = new processes.NuixSearchAndTag{SearchTerm = new Constant<string>("a") , CasePath = new Constant<string>("b") , Tag = new Constant<string>("c") };

            var result = process.Verify(args.settings);

            if (args.expectedError == null)
            {
                result.ShouldBeSuccessful(x=>x.AsString);
            }
            else
            {
                result.ShouldBeFailure(x=>x.AsString, args.expectedError);
            }

        }
    }
}
