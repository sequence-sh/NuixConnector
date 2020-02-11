using Newtonsoft.Json;
using NuixClient.Orchestration;
using NUnit.Framework;
using System.Collections.Generic;

namespace NuixClientTests
{
    public class ProcessJsonTest
    {

        public static readonly List<JsonDeserializeTest> DeserializeTests = new List<JsonDeserializeTest>()
        {
            new JsonDeserializeTest(
                "{\"CaseName\":\"My Case D1\",\"CasePath\":\"Path to my Case\",\"Investigator\":\"Mark\",\"Description\":\"Nice\",\"Type\":\"CreateCaseProcess\",\"Conditions\":null}",

                new CreateCaseProcess
                {
                    CaseName = "My Case D1",
                    CasePath = "Path to my Case",
                    Description = "Nice",
                    Investigator = "Mark"
                }),

            new JsonDeserializeTest(
                "{\"CaseName\":\"My Case D2\",\"CasePath\":\"Path to my Case\",\"Investigator\":\"Mark\",\"Description\":\"Nice\",\"Type\":\"CreateCaseProcess\",\"Conditions\":[{\"FilePath\":\"path\",\"Type\":\"FileExistsCondition\"}]}",


                new CreateCaseProcess
            {
                CaseName = "My Case D2",
                CasePath = "Path to my Case",
                Description = "Nice",
                Investigator = "Mark",
                Conditions = new List<Condition>()
                {
                    new FileExistsCondition
                    {
                        FilePath = "path"
                    }
                }
            }),

            new JsonDeserializeTest(
                "{\"Steps\":[{\"CaseName\":\"My Case D3\",\"CasePath\":\"Path to my Case\",\"Investigator\":\"Mark\",\"Description\":\"Nice\",\"Type\":\"CreateCaseProcess\",\"Conditions\":null},{\"FilePath\":\"Evidence\",\"Custodian\":\"Mark\",\"Description\":\"Cool\",\"FolderName\":\"Evidence\",\"CasePath\":\"Path to my Case\",\"Type\":\"AddFileProcess\",\"Conditions\":null}],\"Type\":\"MultiStepProcess\",\"Conditions\":null}",


                new MultiStepProcess
                {
                    Steps = new List<Process>
                    {
                        new CreateCaseProcess
                        {
                            CaseName = "My Case D3",
                            CasePath = "Path to my Case",
                            Description = "Nice",
                            Investigator = "Mark",
                        },
                        new AddFileProcess
                        {
                            CasePath = "Path to my Case",
                            Custodian = "Mark",
                            Description = "Cool",
                            FolderName = "Evidence",
                            FilePath = "Evidence"
                        }
                    }
                })
        };

        public static readonly List<JsonSerializeTest> SerializeTests = new List<JsonSerializeTest>()
        {
            new JsonSerializeTest(new CreateCaseProcess
            {
                CaseName = "My Case1",
                CasePath = "Path to my Case",
                Description = "Nice",
                Investigator = "Mark"
            }, "{\"CaseName\":\"My Case1\",\"CasePath\":\"Path to my Case\",\"Investigator\":\"Mark\",\"Description\":\"Nice\",\"Type\":\"CreateCaseProcess\",\"Conditions\":null}"),

            new JsonSerializeTest(new CreateCaseProcess
            {
                CaseName = "My Case2",
                CasePath = "Path to my Case",
                Description = "Nice",
                Investigator = "Mark",
                Conditions = new List<Condition>()
                {
                    new FileExistsCondition
                    {
                        FilePath = "path"
                    }
                }
            }, "{\"CaseName\":\"My Case2\",\"CasePath\":\"Path to my Case\",\"Investigator\":\"Mark\",\"Description\":\"Nice\",\"Type\":\"CreateCaseProcess\",\"Conditions\":[{\"FilePath\":\"path\",\"Type\":\"FileExistsCondition\"}]}"),


            new JsonSerializeTest(new MultiStepProcess
            {
                Steps = new List<Process>
                {
                    new CreateCaseProcess()
                    {
                        CaseName = "My Case3",
                        CasePath = "Path to my Case",
                        Description = "Nice",
                        Investigator = "Mark",
                    },
                    new AddFileProcess()
                    {
                        CasePath = "Path to my Case",
                        Custodian = "Mark",
                        Description = "Cool",
                        FolderName = "Evidence",
                        FilePath = "Evidence"
                    }
                }

            },
                "{\"Steps\":[{\"CaseName\":\"My Case3\",\"CasePath\":\"Path to my Case\",\"Investigator\":\"Mark\",\"Description\":\"Nice\",\"Type\":\"CreateCaseProcess\",\"Conditions\":null},{\"FilePath\":\"Evidence\",\"Custodian\":\"Mark\",\"Description\":\"Cool\",\"FolderName\":\"Evidence\",\"CasePath\":\"Path to my Case\",\"Type\":\"AddFileProcess\",\"Conditions\":null}],\"Type\":\"MultiStepProcess\",\"Conditions\":null}")
        };

        [Test]
        [TestCaseSource(nameof(SerializeTests))]
        public void TestSerializeJson(JsonSerializeTest jst)
        {
            var json = JsonConvert.SerializeObject(jst.Process);

            Assert.AreEqual(jst.Expected, json);
        }

        [Test]
        [TestCaseSource(nameof(DeserializeTests))]
        public void TestDeserializeJson(JsonDeserializeTest jdt)
        {
            var obj = JsonConvert.DeserializeObject<Process>(jdt.Json,
                new ProcessJsonConverter(),
                new ConditionJsonConverter());

            var newJson = JsonConvert.SerializeObject(obj);
            var expectedJson = JsonConvert.SerializeObject(jdt.Process);

            Assert.AreEqual(expectedJson, newJson);
        }

    }

    public class JsonDeserializeTest
    {
        public JsonDeserializeTest(string json, Process process)
        {
            Json = json;
            Process = process;
        }

        public string Json { get; }

        public Process Process { get; }

        public override string ToString()
        {
            return Process.GetName();
        }
    }

    public class JsonSerializeTest
    {
        public JsonSerializeTest(Process process, string expected)
        {
            Expected = expected;
            Process = process;
        }

        public string Expected { get; }

        public Process Process { get; }

        public override string ToString()
        {
            return Process.GetName();
        }
    }
}
