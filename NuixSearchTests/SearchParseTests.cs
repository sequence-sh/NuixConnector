using System.Collections.Generic;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using NUnit.Framework.Interfaces;

namespace Reductech.EDR.Connectors.Nuix.Search.Tests
{
    public class SearchParseTests
    {
        public static readonly IReadOnlyList<ITestCase> TestCases = new List<TestCase>()
        {
            new TestCase("*", "*"),
            new TestCase("*.txt", "*.txt"),
            new TestCase("hello", "hello"),
            new TestCase("hello world", "hello AND world"),
            new TestCase("\"hello world\"", "\"hello world\""),
            new TestCase("hello AND world", "hello AND world"),
            new TestCase("hello OR world", "hello OR world"),
            new TestCase("tag:Dinosaurs", "tag:Dinosaurs"),
            new TestCase("tag: Dinosaurs", "tag:Dinosaurs"),
            new TestCase("hello tag:Dinosaurs", "hello AND tag:Dinosaurs"),
            new TestCase("(hello OR greetings) world", "(hello OR greetings) AND world"),
            new TestCase("(nested OR (nest AND ted))", "nested OR (nest AND ted)" ),
            new TestCase("(nested OR (nest AND ted)) AND brackets", "(nested OR (nest AND ted)) AND brackets" ),
            new TestCase("(hello)", "hello"),
            new TestCase("((hello))", "hello"),
            new TestCase("file-size:[1 to 10]", "file-size:[1 TO 10]"),
            new TestCase("file-size:[1 to *]", "file-size:[1 TO *]"),
            new TestCase("file-size:[* to 10]", "file-size:[* TO 10]"),
            new TestCase("date-properties:\"File Modified\":[* TO 1]","date-properties:\"File Modified\":[* TO 1]" ),
            new TestCase("date-properties:\"*\":[* TO 1]","date-properties:\"*\":[* TO 1]" ),
            new TestCase("date-properties:\"*\":[* TO -7Y]","date-properties:\"*\":[* TO -7Y]" ),

            new TestCase("flag:not_processed","flag:not_processed"),

            new TestCase("flag:poison OR (NOT flag:encrypted AND has-embedded-data:0 AND ((has-text:0 AND has-image:0 AND NOT flag:not_processed AND NOT kind:multimedia AND NOT mime-type:application/vnd.ms-shortcut AND NOT mime-type:application/x-contact AND NOT kind:system AND NOT mime-type:( application/vnd.apache-error-log-entry OR application/vnd.git-logstash-log-entry OR application/vnd.linux-syslog-entry OR application/vnd.logstash-log-entry OR application/vnd.ms-iis-log-entry OR application/vnd.ms-windows-event-log-record OR application/vnd.ms-windows-event-logx-record OR application/vnd.ms-windows-setup-api-win7-win8-log-boot-entry OR application/vnd.ms-windows-setup-api-win7-win8-log-section-entry OR application/vnd.ms-windows-setup-api-xp-log-entry OR application/vnd.squid-access-log-entry OR application/vnd.tcpdump.record OR application/vnd.tcpdump.tcp.stream OR application/vnd.tcpdump.udp.stream OR application/vnd.twitter-logstash-log-entry OR application/x-pcapng-entry OR filesystem/x-linux-login-logfile-record OR filesystem/x-ntfs-logfile-record OR server/dropbox-log-event OR text/x-common-log-entry OR text/x-log-entry ) AND NOT kind:log AND NOT mime-type:application/vnd.ms-exchange-stm) OR mime-type:application/vnd.lotus-notes))", 
                "flag:poison OR (NOT flag:encrypted AND has-embedded-data:0 AND ((has-text:0 AND has-image:0 AND NOT flag:not_processed AND NOT kind:multimedia AND NOT mime-type:application/vnd.ms-shortcut AND NOT mime-type:application/x-contact AND NOT kind:system AND NOT mime-type:(application/vnd.apache-error-log-entry OR application/vnd.git-logstash-log-entry OR application/vnd.linux-syslog-entry OR application/vnd.logstash-log-entry OR application/vnd.ms-iis-log-entry OR application/vnd.ms-windows-event-log-record OR application/vnd.ms-windows-event-logx-record OR application/vnd.ms-windows-setup-api-win7-win8-log-boot-entry OR application/vnd.ms-windows-setup-api-win7-win8-log-section-entry OR application/vnd.ms-windows-setup-api-xp-log-entry OR application/vnd.squid-access-log-entry OR application/vnd.tcpdump.record OR application/vnd.tcpdump.tcp.stream OR application/vnd.tcpdump.udp.stream OR application/vnd.twitter-logstash-log-entry OR application/x-pcapng-entry OR filesystem/x-linux-login-logfile-record OR filesystem/x-ntfs-logfile-record OR server/dropbox-log-event OR text/x-common-log-entry OR text/x-log-entry) AND NOT kind:log AND NOT mime-type:application/vnd.ms-exchange-stm) OR mime-type:application/vnd.lotus-notes))"),

            new TestCase("(tag:robot) OR (tag:kitten)", "tag:robot OR tag:kitten"),
            new TestCase("(tag:robot) NOT (tag:kitten)", "tag:robot AND NOT tag:kitten"),
            new TestCase("(tag:robot) AND (tag:kitten)", "tag:robot AND tag:kitten"),
            new TestCase("NOT tag:(robot OR kitten)","NOT tag:(robot OR kitten)" ),


            new TestCase("(tag:robot AND tag:dinosaur) OR (tag:kitten)", "(tag:robot AND tag:dinosaur) OR tag:kitten"),
            new TestCase("(tag:robot AND tag:dinosaur) OR (tag:cute AND tag:kitten)", "(tag:robot AND tag:dinosaur) OR (tag:cute AND tag:kitten)"),
            
            new TestCase("tag:(robots OR ninjas)", "tag:(robots OR ninjas)")

        };

        [Test]
        [TestCaseSource(nameof(TestCases))]
        public void Test(ITestCase testCase)
        {
            testCase.Run();
        }
        
        public static readonly IReadOnlyList<ITestCase> ErrorTestCases = new List<ErrorTestCase>()
        {
            new ErrorTestCase("", "Syntax error: unexpected end of input"),
            new ErrorTestCase("AND", "Syntax error (line 1, column 1): unexpected `AND`"),
            new ErrorTestCase("OR", "Syntax error (line 1, column 1): unexpected `OR`"),
            new ErrorTestCase("NOT", "Syntax error: unexpected end of input"),
            new ErrorTestCase("()", "Syntax error: unexpected end of input"),
            new ErrorTestCase("hello AND", "Syntax error: unexpected end of input"),
            new ErrorTestCase("file-size:[* TO *]", "Either the start or the end of the range must have a value."),
            new ErrorTestCase("file-size:dinosaur", "Could not parse 'dinosaur' as a range"),
        };

        [Test]
        [TestCaseSource(nameof(ErrorTestCases))]
        public void TestError(ITestCase errorTestCase)
        { 
            errorTestCase.Run();
        }

        private class TestCase : ITestCase
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

            /// <inheritdoc />
            public void Run()
            {
                var (isSuccess, _, value, error) = SearchParser.TryParse(Input);

                Assert.IsTrue(isSuccess, error);

                var actual = value.AsString;

                Assert.AreEqual(Expected, actual);
            }
        }

        private class ErrorTestCase : ITestCase
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

            /// <inheritdoc />
            public void Run()
            {
                var (isSuccess, _, _, error) = SearchParser.TryParse(Input);
                Assert.IsFalse(isSuccess, "Expected error");
                StringAssert.StartsWith(ExpectedError, error);
            }
        }

    }

    


}