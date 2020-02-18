using System.Collections.Generic;
using JetBrains.Annotations;
using NuixClient.enums;
using NuixClient.Orchestration;

namespace NuixClient
{
    /// <summary>
    /// Methods for running nuix scripts via the console
    /// </summary>
    public static class OutsideScripting
    {
        /// <summary>
        /// Creates a new Case in NUIX
        /// </summary>
        /// <param name="casePath">Where to create the new case</param>
        /// <param name="searchTerm">The term to search for</param>
        /// <param name="tag">The tag to apply to the found terms</param>
        /// <returns>The output of the case creation script</returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> SearchAndTag( 

            string casePath= @"C:\Dev\Nuix\Cases\NewCase",
            string searchTerm = "night",
            string tag  = "Nocturnal")
        {
            var process = new SearchAndTagProcess
            {
                CasePath = casePath,
                SearchTerm = searchTerm,
                Tag = tag
            };
            await foreach (var r in process.Execute())
            {
                yield return r;
            }
        }

        /// <summary>
        /// Add items to an item set in NUIX
        /// </summary>
        /// <param name="casePath">Where to create the new case</param>
        /// <param name="searchTerm">The term to search for</param>
        /// <param name="itemSetName">The item set to add the found items to. Will be created if it doesn't exist</param>
        /// <param name="itemSetDeduplication">The means of deduplicating items by key and prioritizing originals in a tie-break. </param>
        /// <param name="itemSetDescription">The description of the item set as a string.</param>
        /// <param name="deduplicateBy">Whether to deduplicate as a family or individual</param>
        /// <param name="custodianRanking">A list of custodian names ordered from highest ranked to lowest ranked. If this parameter is present and the deduplication parameter has not been specified, MD5 Ranked Custodian is assumed.</param>
        /// <returns>The output of the case creation script</returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> AddToItemSet(
            string casePath = @"C:\Dev\Nuix\Cases\NewCase",
            string searchTerm = "night",
            string itemSetName = "ProdSet",
            ItemSetDeduplication itemSetDeduplication = ItemSetDeduplication.Default,
            string? itemSetDescription = null,
            DeduplicateBy deduplicateBy = DeduplicateBy.Individual,
            string[]? custodianRanking = null
        )
        {
            var process = new AddToItemSetProcess
            {
                CasePath = casePath,
                SearchTerm = searchTerm,
                ItemSetName = itemSetName,
                ItemSetDeduplication = itemSetDeduplication,
                ItemSetDescription = itemSetDescription,
                DeduplicateBy = deduplicateBy,
                CustodianRanking = custodianRanking
            };


            await foreach (var line in process.Execute())
                yield return line;
        }


        /// <summary>
        /// Add items to a production set in NUIX
        /// </summary>
        /// <param name="casePath">Where to create the new case</param>
        /// <param name="searchTerm">The term to search for</param>
        /// <param name="productionSetName">The production set to add the found items to. Will be created if it doesn't exist</param>
        /// <returns>The output of the case creation script</returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> AddToProductionSet( 

            string casePath= @"C:\Dev\Nuix\Cases\NewCase",
            string searchTerm = "night",
            string productionSetName  = "ProdSet")
        {
            var process = new AddToProductionSetProcess
            {
                CasePath = casePath,
                SearchTerm = searchTerm,
                ProductionSetName = productionSetName
            };

            await foreach (var line in process.Execute())
                yield return line;
        }
        
        /// <summary>
        /// Creates a new Case in NUIX
        /// </summary>
        /// <param name="casePath">Where to create the new case</param>
        /// <param name="caseName">The name of the new case</param>
        /// <param name="description">Description of the case</param>
        /// <param name="investigator">Name of the investigator</param>
        /// <returns>The output of the case creation script</returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> CreateCase(
            string casePath,
            string caseName,
            string description,
            string investigator)
        {
            var process = new CreateCaseProcess()
            {
                CaseName = caseName,
                CasePath = casePath,
                Description = description,
                Investigator = investigator
            };

            await foreach (var line in process.Execute())
                yield return line;
        }

        /// <summary>
        /// Add file or folder to a Case in NUIX
        /// </summary>
        /// <param name="casePath">Path of the case to open</param>
        /// <param name="folderName">The name of the folder to create</param>
        /// <param name="description">Description of the new folder</param>
        /// <param name="custodian">Custodian for the new folder</param>
        /// <param name="filePath">The path of the file to add</param>
        /// <param name="processingProfileName">The name of the processing profile to use. Can be null</param>
        /// <returns>The output of the case creation script</returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> AddFileToCase( //TODO remove default arguments

            string casePath = @"C:\Dev\Nuix\Cases\NewCase",
            string folderName = "TestFolder",
            string description = "nice",
            string custodian = "mark2",
            string filePath = @"C:\Dev\Nuix\Data\Custodians\BobS\Report3.ufdr",
            string? processingProfileName = null)
        {
            var process = new AddFileProcess()
            {
                CasePath = casePath,
                Custodian = custodian,
                Description = description,
                FilePath = filePath,
                FolderName = folderName,
                ProcessingProfileName = processingProfileName
            };

            await foreach (var line in process.Execute())
                yield return line;
        }


        /// <summary>
        /// Add concordance to a case in NUIX
        /// </summary>
        /// <param name="concordanceProfileName">Name of the concordance profile to use</param>
        /// <param name="casePath">Path of the case to open</param>
        /// <param name="folderName">The name of the folder to create</param>
        /// <param name="description">Description of the new folder</param>
        /// <param name="custodian">Custodian for the new folder</param>
        /// <param name="filePath">The path of the file to add</param>
        /// <param name="concordanceDateFormat">Concordance date format to use</param>
        /// <returns>The output of the case creation script</returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> AddConcordanceToCase( //TODO remove default arguments

            string casePath = @"C:\Dev\Nuix\Cases\NewCase",
            string folderName = "BestFolder",
            string description = "nice",
            string custodian = "mw",
            string filePath = @"C:\Dev\Nuix\Exports\Export1\loadfile.dat",
            string concordanceDateFormat = @"yyyy-MM-dd'T'HH:mm:ss.SSSZ",
            string concordanceProfileName = @"TestProfile")
        {
            var process = new AddConcordanceProcess()
            {
                CasePath = casePath,
                ConcordanceDateFormat = concordanceDateFormat,
                ConcordanceProfileName = concordanceProfileName,
                Custodian = custodian,
                Description = description,
                FilePath = filePath,
                FolderName = folderName,

            };

            await foreach (var line in process.Execute())
                yield return line;
        }


        /// <summary>
        /// Export concordance from a production set in NUIX
        /// </summary>
        /// <param name="casePath">Path of the case to open</param>
        /// <param name="exportPath">The path to export to</param>
        /// <param name="productionSetName">The name of the production set to export</param>
        /// <param name="metadataProfileName">Optional name of the metadata profile to use. Case sensitive. Note this is NOT a metadata export profile</param>
        /// <returns>The output of the case creation script</returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> ExportProductionSetConcordance( //TODO remove default arguments
            
            string casePath = @"C:\Dev\Nuix\Cases\NewCase",
            string exportPath = @"C:\Dev\Nuix\Exports\Export6",
            string productionSetName = @"Night",
            string metadataProfileName = "Default")
        {
            var process = new ExportConcordanceProcess
            {
                CasePath = casePath,
                ExportPath = exportPath,
                ProductionSetName = productionSetName,
                MetadataProfileName = metadataProfileName
            };

            await foreach (var line in process.Execute())
                yield return line;
        }
    }
}