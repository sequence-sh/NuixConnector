using System.Collections.Generic;
using CSharpFunctionalExtensions;
using NUnit.Framework;

namespace Reductech.EDR.Connectors.Nuix.Search.Tests
{
    public class SearchMatchTests
    {

        public static List<ITestCase> TestCases = new List<ITestCase>
        {
            //simple
            new TestCase("joe", "joe"),
            new TestCase("joe", " jane", false),
            new TestCase("joe", "regular joe"),
            //Wildcards
            new TestCase("jo?", "joe"),
            new TestCase("j*", "joe"),
            new TestCase("j*", "roe", false),
            new TestCase("*", "joe"),
            new TestCase("n?u*", "neutral" ),
            new TestCase("n?u*", "nutral", false ),
            //Fuzzy
            new TestCase("hot~", "hat"),
            new TestCase("hot~", "shot"),
            new TestCase("hot~", "chat", false),
            //Phrase
            new TestCase("\"Joe Bloggs\"", "Joe Bloggs"),
            new TestCase("\"Joe Bloggs\"", "Joel Bloggs", false),
            new TestCase("\"Joe Bloggs\"", "Joe l Bloggs", false),
            new TestCase("\"Joe Bloggs\"~2",  "Joe John Bloggs"),
            new TestCase("\"Joe* Blog*\"~2",  "Joe John Bloggs"),
            //Exact Query
            new TestCase("'$1,000'", "$1,000"),
            new TestCase("'$1,000'", "$1000", false),
            new TestCase("'1234560000'~2", "123-4560-000"),
            new TestCase("'1234560000'~2", "1-23-4560-000", false),
            //Regex
            new TestCase("/gr[^eao]y/", "gr3y"),
            new TestCase("/gr[^eao]y/", "gray", false),
            new TestCase("/eat|ate apple|orange/~2", "ate red apple"),
            new TestCase("/eat|ate apple|orange/~2", "ate red pear", false),

            //Range
            new TestCase("[joe TO johnathan]", "joe"),
            new TestCase("[joe TO johnathan]", "jonathan"),
            new TestCase("[joe TO johnathan]", "Joffrey"),
            new TestCase("[joe TO johnathan]", "jug", false),
            //Exclusive range
            new TestCase("{joe TO johnathan}", "joe", false), 
            new TestCase("{joe TO johnathan}", "jonathan", false),
            new TestCase("{joe TO johnathan}", "Joffrey"),
            //Mixed range
            new TestCase("[joe TO johnathan}", "joe"),
            new TestCase("[joe TO johnathan}", "jonathan", false),
            //Date range
            new TestCase("comm-date:[20070101 TO 20070131]", "comm-date:20070102"),
            new TestCase("item-date:[20120101 TO *]", "item-date:20120102"),
            //Properties with range
            new TestCase("date-properties:\"File Modified\":[* TO -7Y]", "date-properties:File Modified:2010"),
            new TestCase("date-properties:\"File *\":[* TO -7Y]", "date-properties:File Modified:2010"),
            new TestCase("date-properties:\"File *\":[* TO -7Y]", "date-properties:Item Modified:2010", false),
            new TestCase("date-properties:\"*\":[* TO -7Y]", "date-properties:File Modified:2010"),
            new TestCase("boolean-properties:Compressed:true", "boolean-properties:Compressed:true"),
            new TestCase("boolean-properties:Compressed:true", "boolean-properties:Compressed:false", false),
            new TestCase("boolean-properties:Compressed:false", "boolean-properties:Compressed:false"),
            new TestCase("duration-properties:\"Edit Time\":[60000 TO *]", "duration-properties:Edit Time:60001"),
            //Location
            new TestCase("location-2d:GEODISTANCE((50.0 20.0) 10.0)", "location-2d:50.0, 20.0"),
            new TestCase("location-2d:GEODISTANCE((50.0 20.0) 10.0)", "location-2d:60.0, 20.0"),
            new TestCase("location-2d:GEODISTANCE((50.0 20.0) 10.0)", "location-2d:70.0, 20.0", false),
            new TestCase("location-2d:GEODISTANCE((50.0N 20.0E) 10.0km)", "location-2d:50.0, 20.0"),
            new TestCase("location-2d:GEODISTANCE((50.0N 20.0E) 10.0km)", "location-2d:60.0, 20.0"),
            new TestCase("location-2d:GEODISTANCE((50.0N 20.0E) 10.0km)", "location-2d:70.0, 20.0", false),

            //Field
            new TestCase("name:Embedded", "name:Embedded"),
            new TestCase("name:Embedded", "name:Embedded Item 1"),
            new TestCase("name:Embedded", "item:Embedded", false),
            new TestCase("name:Embedded", "name:Steve", false),
            new TestCase("name:( Embedded AND 1 )","name:Embedded Item 1" ),
            new TestCase("name:( Embedded AND 1 )","name:Embedded Item 2", false ),
            new TestCase("name:( picture* OR itext* )","name:pictures"),
            new TestCase("name:( picture* OR itext* )","name:itextures"),
            new TestCase("name:( picture* OR itext* )","name:ipictures"),

            //AND
            new TestCase("Bloggs Joe", "Joe Joseph Bloggs"),
            new TestCase("Bloggs Joe", "Joel Bloggs", false),
            new TestCase("Bloggs AND Joe", "Joe Joseph Bloggs"),
            new TestCase("Bloggs AND Joe", "Joel Bloggs", false),
            new TestCase("Bloggs +Joe", "Joe Joseph Bloggs"),
            new TestCase("Bloggs +Joe", "Joel Bloggs", false),
            new TestCase("Bloggs J*", "Joel Bloggs"),
            new TestCase("Bloggs Joe~", "Jae Bloggs"),

            //OR
            new TestCase("Bloggs OR Joe", "Steve Bloggs"),
            new TestCase("Bloggs OR Joe", "Steve Smith", false),

            //NOT
            new TestCase("Joe NOT Bloggs", "Joe Smith"),
            new TestCase("Joe NOT Bloggs", "Jack Smith", false),
            new TestCase("Joe NOT Bloggs", "Joe Bloggs", false),
            new TestCase("Joe -Bloggs", "Joe Smith"),
            new TestCase("Joe -Bloggs", "Jack Smith", false),
            new TestCase("Joe -Bloggs", "Joe Bloggs", false),

            //XOR
            new TestCase("( Joe NOT Bloggs ) OR ( Bloggs NOT Joe )", "Joe Smith"),
            new TestCase("( Joe NOT Bloggs ) OR ( Bloggs NOT Joe )", "John Bloggs"),
            new TestCase("( Joe NOT Bloggs ) OR ( Bloggs NOT Joe )", "Joe Bloggs", false),
            new TestCase("( Joe NOT Bloggs ) OR ( Bloggs NOT Joe )", "John Smith", false),

            //Proximity
            new TestCase("Joe W/4 Bloggs", "Joe John James Rodney Bloggs"),
            new TestCase("Joe W/4 Bloggs", "Joe John James Rodney Danger Bloggs", false),
            new TestCase("(John OR Johnny) W/2 Smith", "John Joe Smith"),
            new TestCase("(John OR Johnny) W/2 Smith", "John Bloggs", false),
            new TestCase("(John AND Mary) W/2 Smith", "John Mary Smith"),
            new TestCase("(John AND Mary) W/2 Smith", "Mary John Smith"),
            new TestCase("(John AND Mary) W/2 Smith", "John Mary Smith"),
            new TestCase("(John AND Mary) W/2 Smith", "John Mary Martin Smith"),

            new TestCase("Joe PRE/3 Bloggs", "Joe Joseph Bloggs"),
            new TestCase("Joe PRE/3 Bloggs", "Bloggs Joseph Joe", false),

            //Not Proximity
            new TestCase("\"Acme Corporation\" NOT W/3 Copyright", "Trademark 1990 Acme Corporation"),
            new TestCase("\"Acme Corporation\" NOT W/3 Copyright", "Copyright 1990 Acme Corporation", false),

            new TestCase("Acme NOT PRE/1 ( Corporation OR Inc )", "Acme glue"),
            new TestCase("Acme NOT PRE/1 ( Corporation OR Inc )", "Acme Corporation", false),

            //Operator Grouping
            new TestCase("Joe AND ( Bloggs OR Smith )", "Joe Smith"),
            new TestCase("Joe AND ( Bloggs OR Smith )", "Keith Smith", false),

            //Escaping Special Characters
            new TestCase(@"tag:Blog\*", "tag:Blog*"),
            new TestCase(@"tag:Blog\*", "tag:Blogs", false),

        };

        [Ignore("Feature not implemented yet")]
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

                if(ExpectMatch)
                    Assert.IsTrue(matches, $"'{SearchTerm}' should match '{ObjectToTest}'");
                else
                    Assert.IsFalse(matches, $"'{SearchTerm}' should not match '{ObjectToTest}'");
            }
        }

    }
}
