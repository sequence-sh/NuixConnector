using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public static class Constants
    {
        //TODO set paths from a config file, or something

        public const string Nuix6Path = @"C:\Program Files\Nuix\Nuix 6.2";
        public const string Nuix7Path = @"C:\Program Files\Nuix\Nuix 7.8";
        public const string Nuix8Path = @"C:\Program Files\Nuix\Nuix 8.8";

        public const string NuixConsoleExe = "nuix_console.exe";

        public static List<NuixFeature> AllNuixFeatures => Enum.GetValues(typeof(NuixFeature)).Cast<NuixFeature>().ToList();

        public static IReadOnlyCollection<INuixSettings> NuixSettingsList =>
            new List<INuixSettings>
            {
                new NuixSettings(true, Path.Join(Nuix8Path,NuixConsoleExe ), new Version(8, 8), AllNuixFeatures),
                //new NuixSettings(true, Path.Join(Nuix7Path, NuixConsoleExe), new Version(7, 8), //TODO redo these
                //    AllNuixFeatures),
                //new NuixSettings(true, Path.Join(Nuix6Path, NuixConsoleExe), new Version(6, 2),
                //    AllNuixFeatures),
            };



        public const string Integration = "Integration";
        public const string Category = "Category";

        public static readonly string GeneralDataFolder = Path.Combine(Directory.GetCurrentDirectory(), "IntegrationTest");

        public static readonly IStep<string> CasePath = Constant(Path.Combine(GeneralDataFolder, "TestCase"));
        public static readonly string OutputFolder = Path.Combine(GeneralDataFolder, "OutputFolder");
        public static readonly string ConcordanceFolder = Path.Combine(GeneralDataFolder, "ConcordanceFolder");
        public static readonly IStep<string> NRTFolder = Constant(Path.Combine(GeneralDataFolder, "NRT"));
        public static readonly IStep<string> MigrationTestCaseFolder = Constant(Path.Combine(GeneralDataFolder, "MigrationTest"));

        public static readonly IStep<string> DataPath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "data"));

        public static readonly IStep<string> EncryptedDataPath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "EncryptedData"));

        public static readonly IStep<string> PasswordFilePath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "Passwords.txt"));

        //public static readonly string DefaultOCRProfilePath = Path.Combine(Directory.GetCurrentDirectory(), "AllData", "DefaultOCRProfile.xml");
        public static readonly IStep<string> DefaultProcessingProfilePath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "DefaultProcessingProfile.xml"));
        public static readonly IStep<string> TestProductionProfilePath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "IntegrationTestProductionProfile.xml"));

        public static readonly IStep<string> PoemTextImagePath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "PoemText.png"));
        public static readonly IStep<string> ConcordancePath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "Concordance", "loadfile.dat"));
        public static readonly IStep<string> MigrationPath = Constant(Path.Combine(Directory.GetCurrentDirectory(), "AllData", "MigrationTest.zip"));

        public static readonly IStep<Unit> DeleteCaseFolder = new DeleteItem { Path = CasePath };
        public static readonly IStep<Unit> DeleteOutputFolder = new DeleteItem { Path = Constant(OutputFolder) };
        public static readonly IStep<Unit> CreateOutputFolder = new CreateDirectory { Path = Constant(OutputFolder) };
        public static readonly IStep<Unit> AssertCaseDoesNotExist = new AssertTrue { Test = new Not { Boolean = new NuixDoesCaseExist { CasePath = CasePath } } };
        public static readonly IStep<Unit> CreateCase = new NuixCreateCase
        {
            CaseName = Constant("Integration Test Case"),
            CasePath = CasePath,
            Investigator = Constant("Mark")
        };

        public static IStep<Unit> AssertFileContains(string folderName, string fileName, string expectedContents)
        {
            return new AssertTrue
            {
                Test = new DoesStringContain
                {
                    IgnoreCase = Constant(true),
                    Substring = Constant(expectedContents),
                    Superstring = new FromStream
                    {
                        Stream = new ReadFile
                        {
                            FileName = Constant(fileName),
                            Folder = Constant(folderName)
                        }
                    }
                }
            };
        }

        private static IStep<T> Constant<T>(T c) => new Constant<T>(c);

        public static IStep<Unit> AssertCount(int expected, string searchTerm, IStep<string>? casePath = null) =>
            new AssertTrue
            {
                Test = CompareItemsCount(expected, CompareOperator.Equals, searchTerm, casePath)
            };

        public static IStep<bool> CompareItemsCount(int right, CompareOperator op, string searchTerm, IStep<string>? casePath)
        {
            return new Compare<int>
            {
                Left = new Constant<int>(right),
                Operator = new Constant<CompareOperator>(op),
                Right = new NuixCountItems
                {
                    CasePath = casePath ?? CasePath,
                    SearchTerm = Constant(searchTerm)
                }
            };
        }

        public static readonly IStep<Unit> AddData = new NuixAddItem
        {
            CasePath = CasePath,
            Custodian = Constant("Mark"),
            Path = DataPath,
            FolderName = Constant("New Folder")
        };






    }
}