using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public class RequirementsTest
    {

        public static readonly IReadOnlyCollection<(string? expectedError, NuixProcessSettings settings)> TestCases =
            new List<(string? expectedError, NuixProcessSettings settings)>
            {
                ( "Your version of Nuix (1.0) is less than the required version (5.0) for the process: 'SearchAndTag'",
                    new NuixProcessSettings(true, "abcd", new Version(1,0), new List<NuixFeature>{NuixFeature.ANALYSIS} ) ),

                ( "You lack the required features: 'ANALYSIS' for the process: 'SearchAndTag'",
                    new NuixProcessSettings(true, "abcd", new Version(8,0), new List<NuixFeature>() ) ),

                ("Your version of Nuix (1.0) is less than the required version (5.0) for the process: 'SearchAndTag'\r\nYou lack the required features: 'ANALYSIS' for the process: 'SearchAndTag'",
                    new NuixProcessSettings(true, "abcd", new Version(1,0), new List<NuixFeature>() ) ),

                (null,new NuixProcessSettings(true, "abcd", new Version(8,0), new List<NuixFeature>{NuixFeature.ANALYSIS} ) )
            };

        [TestCaseSource(nameof(TestCases))]
        [Test]
        public void TestRequirements((string? expectedError, NuixProcessSettings settings) args)
        {
            var process = new processes.NuixSearchAndTag{SearchTerm = "a", CasePath = "b", Tag = "c"};

            var freezeResult = process.TryFreeze<Unit>(args.settings);

            if (args.expectedError != null)
            {
                Assert.IsTrue(freezeResult.IsFailure, "Expected freeze to fail");
                freezeResult.Error.Should().Be(args.expectedError);
            }
        }
    }
}
