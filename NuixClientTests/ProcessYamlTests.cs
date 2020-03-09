using System.Collections.Generic;
using CSharpFunctionalExtensions;
using NuixClient.processes;
using NUnit.Framework;
using Processes;
using Processes.enumerations;
using Processes.process;

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
                    new NuixCreateCase
                    {
                        CaseName = "Case Name",
                        CasePath = "Case Path",
                        Description = "Case Description",
                        Investigator = "Investigator"
                    },

                    new NuixAddFile
                    {
                        CasePath = "Case Path",
                        Custodian = "Custodian",
                        Description = "Description",
                        FilePath = "File Path",
                        FolderName = "Folder Name",
                        ProcessingProfileName = "Default"
                    },

                    new NuixCreateReport
                    {
                        CasePath = "Case Path",
                        OutputFolder = "Report Output Folder"
                    },

                    new NuixPerformOCR
                    {
                        CasePath = "Case Path",
                        OCRProfileName = "OCR Profile"
                    },
                    new ForEach
                    {
                        Enumeration = new CSVEnumeration
                        {
                            Delimiter = ",",
                            CSVFilePath = "CSV Path",
                            ColumnInjections = new List<ColumnInjection>
                            {
                                new ColumnInjection
                                {
                                        Header = "SearchTerm",
                                        PropertyToInject = nameof(NuixSearchAndTag.SearchTerm)
                                    },
                                new ColumnInjection
                                {
                                    Header = "Tag",
                                    PropertyToInject = nameof(NuixSearchAndTag.Tag)
                                }
                            }
                        },

                        SubProcess = new NuixSearchAndTag
                        {
                            CasePath = "Case Path"
                        }
                    },

                    new NuixAddToItemSet
                    {
                        CasePath = "Case Path",
                        SearchTerm = "Tag:*",
                        ItemSetName = "TaggedItems"
                    },
                    new NuixAddToProductionSet
                    {
                        SearchTerm = "ItemSet:TaggedItems",
                        ProductionSetName = "Production Set Name",
                        CasePath = "Case Path",
                        Description = "Production Set Description`"
                    },
                    new NuixExportConcordance
                    {
                        CasePath = "Case Path",
                        ExportPath = "Export Path",
                        MetadataProfileName = "Default",
                        ProductionSetName = "Production Set Name"
                    }
                }
            }),

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
                Description = "Interesting",
                FilePath = "C:/MyFile",
                FolderName = "My Folder"
            }),


            new YamlProcessTest(new NuixAddConcordance
            {
                CasePath = "C:/Cases/MyCase",
                Custodian = "Mark",
                Description = "Interesting",
                FilePath = "C:/MyFile",
                FolderName = "My Folder",
                ConcordanceDateFormat = "y",
                ConcordanceProfileName = "Default"
            }),

            new YamlProcessTest(new NuixCreateCase
            {
                CasePath = "C:/Cases/MyCase",
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
                        Description = "Evidence from file",
                        FilePath = "C:/MyFolder",
                        FolderName = "Evidence Folder 1"
                    },

                    new NuixAddConcordance
                    {
                        CasePath = "C:/Cases/MyCase",
                        Custodian = "Mark",
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
            })
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
