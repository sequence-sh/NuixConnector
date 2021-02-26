﻿using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Steps;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class
    NuixGetItemPropertiesTests : NuixStepTestBase<NuixGetItemProperties, StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Get item properties",
                DeleteCaseFolder,
                DeleteOutputFolder,
                CreateOutputFolder,
                CreateCase,
                AddData,
                new AssertTrue
                {
                    Boolean = new StringContains
                    {
                        IgnoreCase = Constant(true),
                        Substring =
                            Constant("Character Set	UTF-8	New Folder/data/Theme in Yellow.txt"),
                        String = new NuixGetItemProperties
                        {
                            PropertyRegex = Constant("(.+)"), SearchTerm = Constant("*")
                        }
                    }
                },
                new AssertTrue
                {
                    Boolean = new StringContains
                    {
                        IgnoreCase = Constant(true),
                        Substring =
                            Constant("Character Set	UTF-8	New Folder/data/Jellyfish.txt"),
                        String = new NuixGetItemProperties
                        {
                            PropertyRegex = Constant("(.+)"),
                            SearchTerm    = Constant("*"),
                            SortSearch    = Constant(true),
                            SearchOptions = Constant(Entity.Create(("limit", 1)))
                        }
                    }
                },
                new NuixCloseConnection(),
                DeleteCaseFolder,
                DeleteOutputFolder
            );
        }
    }
}

}
