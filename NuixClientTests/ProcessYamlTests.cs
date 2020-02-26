using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using NuixClient.Processes;
using NUnit.Framework;
using Orchestration;
using Orchestration.Conditions;
using Orchestration.Enumerations;
using Orchestration.Processes;
using YamlDotNet.Serialization;

namespace NuixClientTests
{
    public class ProcessYamlTest
    {
        public static readonly List<YamlProcessTest> Tests = new List<YamlProcessTest>
        {
            new YamlProcessTest(new Sequence
            {
                Steps = new List<Process>
                {
                    new NuixAddToProductionSet
                    {
                        ProductionSetName = "UnsupportedInfectedPoisoned",
                        SearchTerm = "flag:poison or (NOT flag:encrypted AND has-embedded-data:0 AND ( ( has-text:0 AND has-image:0 AND NOT flag:not_processed AND NOT kind:multimedia AND NOT mime-type:application/vnd.ms-shortcut AND NOT mime-type:application/x-contact AND NOT kind:system AND NOT mime-type:( application/vnd.apache-error-log-entry OR application/vnd.git-logstash-log-entry OR application/vnd.linux-syslog-entry OR application/vnd.logstash-log-entry OR application/vnd.ms-iis-log-entry OR application/vnd.ms-windows-event-log-record OR application/vnd.ms-windows-event-logx-record OR application/vnd.ms-windows-setup-api-win7-win8-log-boot-entry OR application/vnd.ms-windows-setup-api-win7-win8-log-section-entry OR application/vnd.ms-windows-setup-api-xp-log-entry OR application/vnd.squid-access-log-entry OR application/vnd.tcpdump.record OR application/vnd.tcpdump.tcp.stream OR application/vnd.tcpdump.udp.stream OR application/vnd.twitter-logstash-log-entry OR application/x-pcapng-entry OR filesystem/x-linux-login-logfile-record OR filesystem/x-ntfs-logfile-record OR server/dropbox-log-event OR text/x-common-log-entry OR text/x-log-entry ) AND NOT kind:log AND NOT mime-type:application/vnd.ms-exchange-stm ) OR mime-type:application/vnd.lotus-notes ))",
                        CasePath = @"D:\Dev\Nuix\Cases\MarksCase"
                    },
                    new NuixExportConcordance
                    {
                        ProductionSetName = "UnsupportedInfectedPoisoned",
                        ExportPath = @"D:\Dev\Nuix\Exports\MarksCaseTest",
                        CasePath = @"D:\Dev\Nuix\Cases\MarksCase"
                    }
                }
            }),


            new YamlProcessTest(new NuixAddFile
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


            new YamlProcessTest(new NuixAddConcordance
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

            new YamlProcessTest(new NuixCreateCase
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

            new YamlProcessTest(new NuixExportConcordance
            {
                CasePath = "C:/Cases/MyCase",
                ExportPath = "C:/Exports",
                MetadataProfileName = "My Profile",
                ProductionSetName = "Stuff"
            }),

            new YamlProcessTest(new NuixSearchAndTag
            {
                CasePath = "C:/Cases/MyCase",
                SearchTerm = "Raptor",
                Tag = "Dinosaurs"
            }),

            new YamlProcessTest(new NuixAddToProductionSet
            {
                CasePath = "C:/Cases/MyCase",
                SearchTerm = "Raptor",
                ProductionSetName = "Dinosaurs"
            }),

            new YamlProcessTest(new Sequence
            {
                Steps = new List<Process>
                {
                    new NuixCreateCase
                    {
                        CasePath = "C:/Cases/MyCase",
                        Description = "My new case",
                        CaseName = "My Case",
                        Investigator = "Mark"
                    },
                    new NuixAddFile
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

                    new NuixAddConcordance
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
                    new NuixSearchAndTag
                    {
                        CasePath = "C:/Cases/MyCase",
                        SearchTerm = "Raptor",
                        Tag = "Dinosaurs"
                    },
                    new NuixAddToProductionSet
                    {
                        CasePath = "C:/Cases/MyCase",
                        SearchTerm = "Raptor",
                        ProductionSetName = "Dinosaurs"
                    },
                    new NuixExportConcordance
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
                new NuixCreateCase
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
                new NuixSearchAndTag
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
            const string yaml = @"!Sequence
Steps:
- !NuixCreateCase
  CaseName: My Case
  CasePath: &casePath C:/Cases/MyCase
  Investigator: Mark
  Description: &description desc
- !NuixAddFile
  FilePath: C:/MyFolder
  Custodian: Mark
  Description: *description
  FolderName: Evidence Folder 1
  CasePath: *casePath";


            var expectedProcess = new Sequence
            {

                Steps = new List<Process>
                {
                    new NuixCreateCase
                    {
                        CaseName = "My Case",
                        CasePath = "C:/Cases/MyCase",
                        Investigator = "Mark",
                        Description = "desc"
                    },
                    new NuixAddFile
                    {
                        FilePath = "C:/MyFolder",
                        Custodian = "Mark",
                        Description = "desc",
                        FolderName = "Evidence Folder 1",
                        CasePath = "C:/Cases/MyCase"
                    }
                }
            };

            var (isSuccess, _, value, error) = YamlHelper.TryMakeFromYaml(yaml);

            Assert.IsTrue(isSuccess, error);

            Assert.AreEqual(expectedProcess, value);

        }

        [Test]
        [TestCaseSource(nameof(Tests))]
        public void TestYaml(YamlProcessTest yamlProcessTest)
        {
            var yaml = YamlHelper.ConvertToYaml(yamlProcessTest.Process);

            var (isSuccess, _, value, error) = YamlHelper.TryMakeFromYaml(yaml);

            Assert.IsTrue(isSuccess, error);

            Assert.AreEqual(yamlProcessTest.Process, value);
        }

        [Test]
        public async Task TestForeachProcess()
        {
            var list = new List<string>
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

            await foreach (var (isSuccess, _, value, error) in resultList)
            {
                Assert.IsTrue(isSuccess, error);
                realList.Add(value);
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

            public override IEnumerable<string> GetArgumentErrors()
            {
                yield break;
            }

            public override string GetName()
            {
                return "Emit Term";
            }

#pragma warning disable 1998
            public override async IAsyncEnumerable<Result<string>> Execute()
#pragma warning restore 1998
            {
                yield return Result.Success(Term);
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
