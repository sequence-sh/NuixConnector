using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Reductech.EDR.Connectors.Nuix.processes.meta;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public class RequirementsTest
    {

        public static readonly IReadOnlyCollection<(IReadOnlyCollection<string> expectedErrors, NuixProcessSettings settings)> TestCases = 
            new List<(IReadOnlyCollection<string> expectedErrors, NuixProcessSettings settings)>()
            {
                (new  []
                {
                    "Your version of Nuix (1.0) is less than the required version of (2.16) for the process: 'SearchAndTag'"
                },new NuixProcessSettings(true, "abcd", new Version(1,0), new List<NuixFeature>(){NuixFeature.ANALYSIS} ) ),

                (new  []
                {
                    "You lack the required features: 'ANALYSIS' for the process: 'SearchAndTag'"
                },new NuixProcessSettings(true, "abcd", new Version(8,0), new List<NuixFeature>() ) ),

                (new  []
                {
                    "Your version of Nuix (1.0) is less than the required version of (2.16) for the process: 'SearchAndTag'",
                    "You lack the required features: 'ANALYSIS' for the process: 'SearchAndTag'"
                },new NuixProcessSettings(true, "abcd", new Version(1,0), new List<NuixFeature>() ) ),

                (Array.Empty<string>(),new NuixProcessSettings(true, "abcd", new Version(8,0), new List<NuixFeature>(){NuixFeature.ANALYSIS} ) ),


            };

        [TestCaseSource(nameof(TestCases))]
        [Test]
        public void TestRequirements((IReadOnlyCollection<string> expectedErrors, NuixProcessSettings settings) args)
        {
            var process = new processes.NuixSearchAndTag{SearchTerm = "a", CasePath = "b", Tag = "c"};

            var freezeResult = process.TryFreeze(args.settings);

            if (args.expectedErrors.Any())
            {
                Assert.IsTrue(freezeResult.IsFailure, "Expected freeze to fail");
                CollectionAssert.AreEqual( args.expectedErrors, freezeResult.Error);
            }
        }


    }
}
