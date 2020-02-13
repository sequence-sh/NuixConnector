using System.Collections.Generic;
using Newtonsoft.Json;
using NuixClient.Orchestration;
using NUnit.Framework;

namespace NuixClientTests
{
    public class ProcessJsonTest
    {

        public static readonly List<JsonProcessTest> Tests = new List<JsonProcessTest>
        {
            new JsonProcessTest(new AddFileProcess
            {
                CasePath = "C:/Cases/MyCase",
                Custodian = "Mark",
                Conditions = new List<Condition>
                {
                    new FileExistsCondition {FilePath = "C:/MyFile"}
                },
                Description = "Interesting",
                FilePath = "C:/MyFile",
                FolderName = "My Folder"
            }),


            new JsonProcessTest(new AddConcordanceProcess
            {
                CasePath = "C:/Cases/MyCase",
                Custodian = "Mark",
                Conditions = new List<Condition>
                {
                    new FileExistsCondition {FilePath = "C:/MyFile"}
                },
                Description = "Interesting",
                FilePath = "C:/MyFile",
                FolderName = "My Folder",
                ConcordanceDateFormat = "y",
                ConcordanceProfileName = "Default"
            }),

            new JsonProcessTest(new CreateCaseProcess
            {
                CasePath = "C:/Cases/MyCase",
                Conditions = new List<Condition>
                {
                    new FileExistsCondition {FilePath = "C:/MyFile"}
                },
                Description = "Interesting",
                CaseName = "My Case",
                Investigator = "Mark"
            }),

            new JsonProcessTest(new ExportConcordanceProcess
            {
                CasePath = "C:/Cases/MyCase",
                ExportPath = "C:/Exports",
                MetadataProfileName = "My Profile",
                ProductionSetName = "Stuff"
            }),

            new JsonProcessTest(new SearchAndTagProcess
            {
                CasePath = "C:/Cases/MyCase",
                SearchTerm = "Raptor",
                Tag = "Dinosaurs"
            }),

            new JsonProcessTest(new AddToProductionSetProcess()
            {
                CasePath = "C:/Cases/MyCase",
                SearchTerm = "Raptor",
                ProductionSetName = "Dinosaurs"
            }),

            new JsonProcessTest(new MultiStepProcess
            {
                Steps = new List<Process>
                {
                    new CreateCaseProcess
                    {
                        CasePath = "C:/Cases/MyCase",
                        Conditions = new List<Condition>
                        {
                            new FileExistsCondition {FilePath = "C:/MyFile"}
                        },
                        Description = "Interesting",
                        CaseName = "My Case",
                        Investigator = "Mark"
                    },
                    new SearchAndTagProcess
                    {
                        CasePath = "C:/Cases/MyCase",
                        SearchTerm = "Raptor",
                        Tag = "Dinosaurs"
                    }
                }
            }),

            new JsonProcessTest(new BranchProcess{Options = new List<Process>
            {
                new CreateCaseProcess
                {
                    CasePath = "C:/Cases/MyCase",
                    Conditions = new List<Condition>
                    {
                        new FileExistsCondition {FilePath = "C:/MyFile"}
                    },
                    Description = "Interesting",
                    CaseName = "My Case",
                    Investigator = "Mark"
                },
                new SearchAndTagProcess
                {
                    CasePath = "C:/Cases/MyCase",
                    SearchTerm = "Raptor",
                    Tag = "Dinosaurs"
                }
            }})
        };

        [Test]
        [TestCaseSource(nameof(Tests))]
        public void TestJson(JsonProcessTest jsonProcessTest)
        {
            var json = JsonConvert.SerializeObject(jsonProcessTest.Process);

            var obj = JsonConvert.DeserializeObject<Process>(json,
                new ProcessJsonConverter(),
                new ConditionJsonConverter());

            Assert.AreEqual(jsonProcessTest.Process, obj);
        }

    }

    /// <summary>
    /// Tests serialization and deserialization of a process
    /// </summary>
    public class JsonProcessTest
    {
        internal JsonProcessTest(Process process)
        {
            Process = process;
        }

        /// <summary>
        /// The process to test
        /// </summary>
        internal Process Process { get; }

        public override string ToString()
        {
            return Process.GetName();
        }
    }

}
