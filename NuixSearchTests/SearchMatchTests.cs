using System.Collections.Generic;
using CSharpFunctionalExtensions;
using NUnit.Framework;

namespace Reductech.EDR.Connectors.Nuix.Search.Tests
{
    public class SearchMatchTests
    {

        public static List<ITestCase> TestCases = new List<ITestCase>
        {
            new TestCase("joe", "name: joe"),
            new TestCase("joe", "name: joe"),
            new TestCase("joe", "name: regular joe"),
            new TestCase("jo?", "name: joe"),
            new TestCase("j*", "name: joe"),
            new TestCase("j*", "name: roe"),
            new TestCase("*", "name: joe"),
        };

        [Test]
        [TestCaseSource(nameof(TestCases))]
        public void RunTest(ITestCase testCase)
        {
            testCase.Run();
        }


        private class TestCase : ITestCase
        {
            public TestCase(string searchTerm, string objectToTest, bool expectMatch = true)
            {
                SearchTerm = searchTerm;
                ObjectToTest = objectToTest;
                ExpectMatch = expectMatch;
            }

            public string SearchTerm { get; }

            public string ObjectToTest { get; }

            public bool ExpectMatch { get; }

            public void Run()
            {
                var so = SearchableObject.FromString(ObjectToTest);

                var (isSuccess, _, search, error) = SearchParser.TryParse(SearchTerm);

                Assert.IsTrue(isSuccess, error);

                var matches = search.Matches(so);

                Assert.IsTrue(matches, $"'{SearchTerm}' should match '{ObjectToTest}'");
            }
        }

    }
}
