using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using NuixClient;
using NuixClient.Orchestration;
using NuixClient.Orchestration.Conditions;
using NuixClient.Orchestration.Enumerations;
using NuixClient.Orchestration.Processes;
using NUnit.Framework;
using YamlDotNet.Serialization;

namespace NuixClientTests
{
    public class ProcessYamlTest
    {
        public static readonly List<YamlProcessTest> Tests = new List<YamlProcessTest>
        {
            new YamlProcessTest(new AddFile
            {
                CasePath = "C:/Cases/MyCase",
                Custodian = "Mark",
                Conditions = new List<Condition>
                {
                    new FileExists {FilePath = "C:/MyFile"}
                },
                Description = "Interesting",
                FilePath = "C:/MyFile",
                FolderName = "My Folder"
            }),


            new YamlProcessTest(new AddConcordance
            {
                CasePath = "C:/Cases/MyCase",
                Custodian = "Mark",
                Conditions = new List<Condition>
                {
                    new FileExists {FilePath = "C:/MyFile"}
                },
                Description = "Interesting",
                FilePath = "C:/MyFile",
                FolderName = "My Folder",
                ConcordanceDateFormat = "y",
                ConcordanceProfileName = "Default"
            }),

            new YamlProcessTest(new CreateCase
            {
                CasePath = "C:/Cases/MyCase",
                Conditions = new List<Condition>
                {
                    new FileExists {FilePath = "C:/MyFile"}
                },
                Description = "Interesting",
                CaseName = "My Case",
                Investigator = "Mark"
            }),

            new YamlProcessTest(new ExportConcordance
            {
                CasePath = "C:/Cases/MyCase",
                ExportPath = "C:/Exports",
                MetadataProfileName = "My Profile",
                ProductionSetName = "Stuff"
            }),

            new YamlProcessTest(new SearchAndTag
            {
                CasePath = "C:/Cases/MyCase",
                SearchTerm = "Raptor",
                Tag = "Dinosaurs"
            }),

            new YamlProcessTest(new AddToProductionSet()
            {
                CasePath = "C:/Cases/MyCase",
                SearchTerm = "Raptor",
                ProductionSetName = "Dinosaurs"
            }),

            new YamlProcessTest(new Sequence
            {
                Steps = new List<Process>
                {
                    new CreateCase
                    {
                        CasePath = "C:/Cases/MyCase",
                        Description = "My new case",
                        CaseName = "My Case",
                        Investigator = "Mark"
                    },
                    new AddFile
                    {
                        CasePath = "C:/Cases/MyCase",
                        Custodian = "Mark",
                        Conditions = new List<Condition>
                        {
                            new FileExists {FilePath = "C:/MyFolder"}
                        },
                        Description = "Evidence from file",
                        FilePath = "C:/MyFolder",
                        FolderName = "Evidence Folder 1"
                    },

                    new AddConcordance
                    {
                        CasePath = "C:/Cases/MyCase",
                        Custodian = "Mark",
                        Conditions = new List<Condition>
                        {
                            new FileExists {FilePath = "C:/MyConcordance.dat"}
                        },
                        Description = "Evidence from concordance",
                        FilePath = "C:/MyConcordance.dat",
                        FolderName = "Evidence Folder 2",
                        ConcordanceDateFormat = "yyyy-MM-dd'T'HH:mm:ss.SSSZ",
                        ConcordanceProfileName = "Default"
                    },
                    new SearchAndTag
                    {
                        CasePath = "C:/Cases/MyCase",
                        SearchTerm = "Raptor",
                        Tag = "Dinosaurs"
                    },
                    new AddToProductionSet()
                    {
                        CasePath = "C:/Cases/MyCase",
                        SearchTerm = "Raptor",
                        ProductionSetName = "Dinosaurs"
                    },
                    new ExportConcordance
                    {
                        CasePath = "C:/Cases/MyCase",
                        ExportPath = "C:/Exports",
                        MetadataProfileName = "Default",
                        ProductionSetName = "Dinosaurs"
                    }
                }
            }),

            new YamlProcessTest(new Branch{Options = new List<Process>
            {
                new CreateCase
                {
                    CasePath = "C:/Cases/MyCase",
                    Conditions = new List<Condition>
                    {
                        new FileExists {FilePath = "C:/MyFile"}
                    },
                    Description = "Interesting",
                    CaseName = "My Case",
                    Investigator = "Mark"
                },
                new SearchAndTag
                {
                    CasePath = "C:/Cases/MyCase",
                    SearchTerm = "Raptor",
                    Tag = "Dinosaurs"
                }
            }})
        };

        [Test]
        public void TestYamlAnchors()
        {
            var yaml = @"!Sequence
Steps:
- !CreateCase
  CaseName: My Case
  CasePath: &casePath C:/Cases/MyCase
  Investigator: Mark
  Description: &description desc
- !AddFile
  FilePath: C:/MyFolder
  Custodian: Mark
  Description: *description
  FolderName: Evidence Folder 1
  CasePath: *casePath";


            var expectedProcess = new Sequence()
            {

                Steps = new List<Process>()
                {
                    new CreateCase()
                    {
                        CaseName = "My Case",
                        CasePath = "C:/Cases/MyCase",
                        Investigator = "Mark",
                        Description = "desc"
                    },
                    new AddFile()
                    {
                        FilePath = "C:/MyFolder",
                        Custodian = "Mark",
                        Description = "desc",
                        FolderName = "Evidence Folder 1",
                        CasePath = "C:/Cases/MyCase"
                    }
                }
            };

            var success = YamlHelper.TryMakeFromYaml(yaml, out var p, out var e);

            Assert.IsTrue(success, e);

            Assert.AreEqual(expectedProcess, p);

        }

        [Test]
        [TestCaseSource(nameof(Tests))]
        public void TestYaml(YamlProcessTest yamlProcessTest)
        {
            var yaml = YamlHelper.ConvertToYaml(yamlProcessTest.Process);

            var success = YamlHelper.TryMakeFromYaml(yaml, out var p, out var e);

            Assert.IsTrue(success, e);

            Assert.AreEqual(yamlProcessTest.Process, p);
        }

        [Test]
        public async Task TestForeachProcess()
        {
            var list = new List<string>()
            {
                "Correct", "Horse", "Battery", "Staple"
            };
            var expected = list.Select(s => $"'{s}'").ToList();

            var forEachProcess = new ForEach
            {
                Enumeration = new Collection{Members = list},
                PropertyToInject = nameof(EmitTermProcess.Term),
                Template = "'$s'",
                SubProcess = new EmitTermProcess()
            };

            var realList = new List<string>();

            var resultList = forEachProcess.Execute();

            await foreach (var s in resultList)
            {
                Assert.IsTrue(s.IsSuccess);
                realList.Add(s.Line);
            }

            CollectionAssert.AreEqual(expected, realList);
            
        }

        private class EmitTermProcess : Process
        {
            [UsedImplicitly]
            [YamlMember]
            [Required]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
            public string Term { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

            internal override IEnumerable<string> GetArgumentErrors()
            {
                yield break;
            }

            public override string GetName()
            {
                return "Emit Term";
            }

#pragma warning disable 1998
            public override async IAsyncEnumerable<ResultLine> Execute()
#pragma warning restore 1998
            {
                yield return new ResultLine(true, Term);
            }
        }

    }

    /// <summary>
    /// Tests serialization and deserialization of a process
    /// </summary>
    public class YamlProcessTest
    {
        internal YamlProcessTest(Process process)
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
