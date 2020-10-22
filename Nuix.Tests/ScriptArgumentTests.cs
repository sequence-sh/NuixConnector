using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using Reductech.EDR.Connectors.Nuix.Conversion;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

    public class ScriptArgumentTests : ScriptArgumentTestCases
    {
        public ScriptArgumentTests(ITestOutputHelper testOutputHelper) => TestOutputHelper = testOutputHelper;

        /// <inheritdoc />
        [Theory]
        [ClassData(typeof(ScriptArgumentTestCases))]
        public override void Test(string key) => base.Test(key);
    }

    public class ScriptArgumentTestCases : TestBase
    {
        /// <inheritdoc />
        protected override IEnumerable<ITestBaseCase> TestCases {
            get
            {
                yield return new ScriptArgumentTestCase("Case with Description",
                    new NuixCreateCase
                {
                    CaseName = new Constant<string>("Case"),
                    CasePath = new Constant<string>("Case Path"),
                    Description = new Constant<string>("Description"),
                    Investigator = new Constant<string>("Investigator")

                });

                yield return new ScriptArgumentTestCase("Case with no Description", new NuixCreateCase
                {
                    CaseName = new Constant<string>("Case"),
                    CasePath = new Constant<string>("Case Path"),
                    Investigator = new Constant<string>("Investigator")

                });


                yield return new ScriptArgumentTestCase("If Then", new Conditional
                {
                    Condition = new Constant<bool>(true),
                    ThenStep = new NuixCreateCase()
                    {
                        CaseName = new Constant<string>("Case"),
                        CasePath = new Constant<string>("Case Path"),
                        Description = new Constant<string>("Description"),
                        Investigator = new Constant<string>("Investigator")
                    }
                });

                yield return new ScriptArgumentTestCase("If Then Else", new Conditional
                {
                    Condition = new Constant<bool>(true),
                    ThenStep = new NuixCreateCase()
                    {
                        CaseName = new Constant<string>("Case"),
                        CasePath = new Constant<string>("Case Path"),
                        Description = new Constant<string>("Description"),
                        Investigator = new Constant<string>("Investigator")
                    },
                    ElseStep = new NuixCreateCase()
                    {
                        CaseName = new Constant<string>("Case"),
                        CasePath = new Constant<string>("Case Path"),
                        Description = new Constant<string>("Description"),
                        Investigator = new Constant<string>("Investigator")
                    }
                });
            }
        }


        private sealed class ScriptArgumentTestCase : ITestBaseCase
        {
            public ScriptArgumentTestCase(string name, IStep step)
            {
                Name = name;
                RubyBlockStep = step;
            }

            public IStep RubyBlockStep { get; }


            /// <inheritdoc />
            public string Name { get; }

            private static readonly Regex ArgRegex = new Regex(@"\w+(\d[a-z])*\d[a-z]?", RegexOptions.Compiled | RegexOptions.IgnoreCase);

            /// <inheritdoc />
            public void Execute(ITestOutputHelper testOutputHelper)
            {
                var r = RubyBlockConversion.TryConvert(RubyBlockStep, "Parameter");

                r.ShouldBeSuccessful();

                var block = r.Value;

                var argsResult = block.TryGetArguments(new Suffixer());
                argsResult.MapError(x=>x.AsString).ShouldBeSuccessful();

                var args = argsResult.Value.ToList();


                var allArguments = new List<string>();

                for (var i = 0; i < argsResult.Value.Count; i+=2)
                {
                    var arg = args[i];
                    var match = ArgRegex.Match(arg);
                    match.Success.Should().BeTrue($"{arg} should be an argument name");
                    allArguments.Add(match.Value);
                }

                allArguments.Should().OnlyHaveUniqueItems("arguments should be unique");

                var optParseLines = new IndentationStringBuilder(new StringBuilder(),0);

                block.WriteOptParseLines("args", optParseLines, new Suffixer());

                var optParseArguments = ArgRegex.Matches(optParseLines.StringBuilder.ToString())
                    .Select(x => x.Value).Distinct().ToList();


                optParseArguments.Should().Contain(allArguments);//There may be opt parse lines that do not map to particular arguments


                var blockLines = new IndentationStringBuilder(new StringBuilder(), 0);

                if (block is ITypedRubyBlock typedRubyBlock)
                {
                    typedRubyBlock.TryWriteBlockLines(new Suffixer(), blockLines)
                        .MapError(x=>x.AsString)
                        .ShouldBeSuccessful();
                }
                else if (block is IUnitRubyBlock unitRubyBlock)
                {
                    unitRubyBlock.TryWriteBlockLines(new Suffixer(), blockLines)
                        .MapError(x => x.AsString)
                        .ShouldBeSuccessful();
                }
                else
                    throw new XunitException("Block is not a typed block or a unit block");

                var blockArguments = ArgRegex.Matches(blockLines.StringBuilder.ToString())
                    .Select(x => x.Value).Distinct().ToList();

                blockArguments.Should().BeEquivalentTo(optParseArguments);
            }
        }
    }
}
