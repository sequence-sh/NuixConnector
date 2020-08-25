//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using CSharpFunctionalExtensions;
//using NUnit.Framework;
//using Reductech.EDR.Connectors.Nuix.processes;
//using Reductech.EDR.Connectors.Nuix.processes.meta;
//using Reductech.EDR.Processes;
//using Reductech.EDR.Processes.General;

//namespace Reductech.EDR.Connectors.Nuix.Tests
//{
//    public class ProcessConversionTests
//    {
//        public static readonly List<YamlProcessTest> Tests = new List<YamlProcessTest>
//        {
//            new YamlProcessTest(
//                new NuixSearchAndTag
//                {
//                    SearchTerm = "s",
//                    Tag = "t",
//                    CasePath = "cp"
//                }),

//            new YamlProcessTest(new Sequence
//            {
//                Steps = new List<Process>
//                {
//                    new NuixCreateCase
//                    {
//                        CasePath = "cp",
//                        CaseName = "MyCase",
//                        Investigator = "Mark"
//                    },
//                    new NuixSearchAndTag
//                    {
//                        SearchTerm = "s",
//                        Tag = "t",
//                        CasePath = "cp"
//                    }
//                }
//            }),


//            new YamlProcessTest(new Conditional
//            {
//                If = new NuixDoesCaseExists
//                {
//                    CasePath = "cp",

//                },
//                Then = new NuixSearchAndTag
//                {
//                    SearchTerm = "s",
//                    Tag = "t",
//                    CasePath = "cp"
//                },
//                Else = new Sequence
//                {
//                    Steps = new List<Process>
//                    {
//                        new NuixCreateCase
//                        {
//                            CasePath = "cp",
//                            CaseName = "MyCase",
//                            Investigator = "Mark"
//                        },
//                        new NuixSearchAndTag
//                        {
//                            SearchTerm = "s",
//                            Tag = "t",
//                            CasePath = "cp"
//                        }
//                    }
//                }
//            }),

//            new YamlProcessTest(new Sequence
//            {
//                Steps = new List<Process>
//                {
//                    new NuixCreateCase
//                    {
//                        CaseName = "Case Name",
//                        CasePath = "Case Path",
//                        Description = "Case Description",
//                        Investigator = "Investigator"
//                    },

//                    new NuixAddItem
//                    {
//                        CasePath = "Case Path",
//                        Custodian = "Custodian",
//                        Description = "Description",
//                        Path = "File Path",
//                        FolderName = "Folder Name",
//                        ProcessingProfileName = "Default"
//                    },

//                    new WriteFile
//                    {
//                        Text = new NuixCreateReport
//                        {
//                            CasePath = "Case Path",

//                        },
//                        Folder = "Report Output Folder",
//                        FileName = "Report.csv"
//                    },

//                    new NuixPerformOCR
//                    {
//                        CasePath = "Case Path",
//                        OCRProfileName = "OCR Profile"
//                    },
//                    new Loop
//                    {
//                        For = new CSV
//                        {
//                            Delimiter = ",",
//                            CSVText = @"SearchTerm,Tag
//Raptor,Dinosaur
//Red,Color",
//                            ColumnInjections = new List<ColumnInjection>()
//                            {
//                                new ColumnInjection{Column = "SearchTerm", Property = nameof(NuixSearchAndTag.SearchTerm) },
//                                new ColumnInjection{Column = "Tag", Property = nameof(NuixSearchAndTag.Tag) },
//                            },
//                        },
//                        Do = new NuixSearchAndTag
//                        {
//                            CasePath = "Case Path"
//                        }
//                    },

//                    new NuixAddToItemSet
//                    {
//                        CasePath = "Case Path",
//                        SearchTerm = "Tag:*",
//                        ItemSetName = "TaggedItems"
//                    },
//                    new NuixAddToProductionSet
//                    {
//                        SearchTerm = "ItemSet:TaggedItems",
//                        ProductionSetName = "Production Set Name",
//                        CasePath = "Case Path",
//                        Description = "Production Set Description`",
//                        ProductionProfileName = "Default"
//                    },
//                    new NuixExportConcordance
//                    {
//                        CasePath = "Case Path",
//                        ExportPath = "Export Path",
//                        ProductionSetName = "Production Set Name"
//                    }
//                }
//            })
//        };

//        private static readonly INuixProcessSettings NuixProcessSettings =
//            new NuixProcessSettings(true, "abcd", new Version(8, 2),
//                Enum.GetValues(typeof(NuixFeature)).Cast<NuixFeature>().ToList());


//        [Test]
//        [TestCaseSource(nameof(Tests))]
//        public void TestProcessConversion(YamlProcessTest yamlProcessTest)
//        {
//            var (isSuccess, _, freezeValue, freezeError) = yamlProcessTest.Process.TryFreeze<Unit>(NuixProcessSettings);

//            Assert.IsTrue(isSuccess, freezeError);
//            Assert.IsInstanceOf<IRubyScriptProcess>(freezeValue);
//        }
//    }
//}
