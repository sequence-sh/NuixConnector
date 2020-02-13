using System.Collections.Generic;
using NUnit.Framework;

namespace NuixClientTests
{
    public class SearchParseTests
    {
        [SetUp]
        public void Setup()
        {
        }

        public static readonly IReadOnlyList<TestCase> TestCases = new List<TestCase>()
        {
            new TestCase("hello", "hello"),
            new TestCase("hello world", "hello AND world"),
            new TestCase("\"hello world\"", "\"hello world\""),
            new TestCase("hello AND world", "hello AND world"),
            new TestCase("hello OR world", "hello OR world"),
            new TestCase("tag:Dinosaurs", "tag:Dinosaurs"),
            new TestCase("hello tag:Dinosaurs", "hello AND tag:Dinosaurs"),
            new TestCase("(hello OR greetings) world", "(hello OR greetings) AND world"),
            new TestCase("(nested OR (nest AND ted))", "nested OR (nest AND ted)" ),
            new TestCase("(nested OR (nest AND ted)) AND brackets", "(nested OR (nest AND ted)) AND brackets" ),
            new TestCase("(hello)", "hello"),
            new TestCase("((hello))", "hello"),
            new TestCase("file-size:[1 to 10]", "file-size:[1 TO 10]"),
            new TestCase("file-size:[1 to *]", "file-size:[1 TO *]"),
            new TestCase("file-size:[* to 10]", "file-size:[* TO 10]"),
        };

        [Test]
        [TestCaseSource(nameof(TestCases))]
        public void Test(TestCase testCase)
        {
            var (success, error, result) = NuixClient.Search.SearchParser.TryParse(testCase.Input);

            Assert.IsTrue(success, error);
            Assert.IsNotNull(result);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            Assert.IsEmpty(result.ErrorMessages);
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            var actual = result.AsString;

            Assert.AreEqual(testCase.Expected, actual);
        }
        
        public static readonly IReadOnlyList<ErrorTestCase> ErrorTestCases = new List<ErrorTestCase>()
        {
            new ErrorTestCase("", "Syntax error: unexpected end of input"),
            new ErrorTestCase("AND", "Syntax error (line 1, column 1): unexpected `AND`"),
            new ErrorTestCase("OR", "Syntax error (line 1, column 1): unexpected `OR`"),
            new ErrorTestCase("NOT", "Syntax error: unexpected end of input"),
            new ErrorTestCase("()", "Syntax error: unexpected end of input"),
            new ErrorTestCase("hello AND", "Syntax error: unexpected end of input"),
            new ErrorTestCase("File-size:[* TO *]", "'[* TO *]' is an invalid value for 'File-size'"),
            new ErrorTestCase("File-size:dinosaur", "'dinosaur' is an invalid value for 'File-size'"),
            //new ErrorTestCase("Tag:[5 TO 9]", "'[5 TO 9]' is an invalid value for 'Tags'"),
        };

        [Test]
        [TestCaseSource(nameof(ErrorTestCases))]
        public void TestError(ErrorTestCase errorTestCase)
        {
            var (success, error, _) = NuixClient.Search.SearchParser.TryParse(errorTestCase.Input);

            Assert.IsFalse(success, "Expected error");
            Assert.IsNotNull(error);


            StringAssert.StartsWith(errorTestCase.ExpectedError, error);
        }

        
    }

    public class TestCase
    {
        public readonly string Input;
        public readonly string Expected;

        public TestCase(string input, string expected)
        {
            Input = input;
            Expected = expected;
        }

        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Input))
                return "[EMPTY]";
            return Input;
        }
    }

    public class ErrorTestCase
    {
        public readonly string Input;
        public readonly string ExpectedError;

        public ErrorTestCase(string input, string expectedError)
        {
            Input = input;
            ExpectedError = expectedError;
        }


        public override string ToString()
        {
            if (string.IsNullOrWhiteSpace(Input))
                return "[EMPTY]";

            return Input;
        }
    }


}