﻿using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.Utilities.Testing;
using Xunit;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

public class RequirementsTest
{
    public static readonly TheoryData<(string? expectedError, NuixSettings settings)> TestCases =
        new TheoryData<(string? expectedError, NuixSettings settings)>
        {
            ("Required Nuix Version >= 5.0 but had 1.0",
             new NuixSettings(
                 true,
                 "abcd",
                 new Version(1, 0),
                 new List<NuixFeature> { NuixFeature.ANALYSIS }
             )),
            ("ANALYSIS missing",
             new NuixSettings(true, "abcd", new Version(8, 0), new List<NuixFeature>())),
            ("Required Nuix Version >= 5.0 but had 1.0; ANALYSIS missing",
             new NuixSettings(true, "abcd", new Version(1, 0), new List<NuixFeature>())),
            (null,
             new NuixSettings(
                 true,
                 "abcd",
                 new Version(8, 0),
                 new List<NuixFeature> { NuixFeature.ANALYSIS }
             ))
        };

    [Theory]
    [MemberData(nameof(TestCases))]
    public void TestRequirements((string? expectedError, NuixSettings settings) args)
    {
        var process = new NuixSearchAndTag
        {
            SearchTerm = Constant("a"), CasePath = Constant("b"), Tag = Constant("c")
        };

        var result = process.Verify(args.settings);

        if (args.expectedError == null)
            result.ShouldBeSuccessful(x => x.AsString);
        else
            result.MapError(x => x.AsString).ShouldBeFailure(args.expectedError);
    }
}

}
