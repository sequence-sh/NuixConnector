using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using NuixClient;

namespace NuixClientAPI.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class NuixScriptController : ControllerBase
    {
        //TODO gRPC
        private static IActionResult ConvertToActionResult(IAsyncEnumerable<ResultLine> asyncEnumerable)
        {
            var enumerator = asyncEnumerable.GetAsyncEnumerator();
            var sb = new StringBuilder();
            var allGood = true;
            try
            {
                while (enumerator.MoveNextAsync().Result)
                {
                    sb.AppendLine(enumerator.Current.Line);
                    allGood &= enumerator.Current.IsSuccess;
                }

                if(allGood)
                    return new ContentResult{Content = sb.ToString()};
                return new BadRequestObjectResult(sb.ToString());
            }
#pragma warning disable CA1031 //We don't know what kind of exception could be thrown here
            catch (Exception e)
            {
                return new BadRequestObjectResult(e.Message);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }

        /// <summary>
        /// Creates a new Case in NUIX
        /// </summary>
        /// <param name="casePath">Where to create the new case</param>
        /// <param name="searchTerm">The term to search for</param>
        /// <param name="tag">The tag to add to the case</param>
        /// <param name="order">Order by term e.g. name ASC</param>
        /// <param name="limit">Optional maximum number of items to tag.</param>
        /// <returns>The output of the case creation script</returns>
        [HttpPost("/SearchAndTagProcess")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Swagger UI can't see static methods")]
        public IActionResult SearchAndTag(
            string casePath,
            string searchTerm,
            string tag,
            string order,
            int? limit)
        {
            var r = OutsideScripting.SearchAndTag(casePath, searchTerm, tag, order, limit);

            return ConvertToActionResult(r);
        }

        /// <summary>
        /// Creates a new Case in NUIX
        /// </summary>
        /// <param name="casePath">Where to create the new case</param>
        /// <param name="caseName">The name of the new case</param>
        /// <param name="description">Description of the case</param>
        /// <param name="investigator">Name of the investigator</param>
        /// <returns>The output of the case creation script</returns>
        [HttpPost("/CreateCase")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Swagger UI can't see static methods")]
        public IActionResult CreateCase(string casePath,
            string caseName, 
            string description,
            string investigator)
        {
            var r = OutsideScripting.CreateCase(casePath, caseName, description, investigator);

            return ConvertToActionResult(r);
        }

        /// <summary>
        /// Add file or folder to a Case in NUIX
        /// </summary>
        /// <param name="casePath">Path of the case to open</param>
        /// <param name="folderName">The name of the folder to create</param>
        /// <param name="description">Description of the new folder</param>
        /// <param name="custodian">Custodian for the new folder</param>
        /// <param name="filePath">The path of the file to add</param>
        /// <param name="processingProfileName">The name of the processing profile to use. Can be null.</param>
        /// <returns>The output of the case creation script</returns>
        [HttpPost("/AddFile")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Swagger UI can't see static methods")]
        public IActionResult AddFileToCase(
            string casePath,
            string folderName,
            string description,
            string custodian,
            string filePath,
            string processingProfileName
            )
        {
            var r = 
                OutsideScripting.AddFileToCase(
                casePath, 
                folderName, 
                description, 
                custodian, 
                filePath,
                processingProfileName);

            return ConvertToActionResult(r);
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
        [HttpPost("/AddConcordance")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Swagger UI can't see static methods")]
        public IActionResult AddConcordanceToCase(
            string casePath,
            string folderName,
            string description,
            string custodian,
            string filePath,
            string concordanceDateFormat,
            string concordanceProfileName)
        {
            var r = OutsideScripting.AddConcordanceToCase(
                casePath, folderName, description, custodian,
                filePath, concordanceDateFormat, concordanceProfileName);

            return ConvertToActionResult(r);
        }


        /// <summary>
        /// Export concordance from a production set in NUIX
        /// </summary>
        /// <param name="casePath">Path of the case to open</param>
        /// <param name="exportPath">The path to export to</param>
        /// <param name="productionSetName">The name of the production set to export</param>
        /// <param name="metadataProfileName">Optional name of the metadata profile to use. Case sensitive. Note this is NOT a metadata export profile</param>
        
        /// ///
        /// <returns>The output of the case creation script</returns>
        [HttpPost("/ExportConcordanceProcess")]
        [SuppressMessage("Performance", "CA1822:Mark members as static", Justification = "Swagger UI can't see static methods")]
        public IActionResult ExportProductionSetConcordance(
            string casePath,
            string exportPath ,
            string productionSetName,
            string metadataProfileName)
        {
            var r = OutsideScripting.ExportProductionSetConcordance(casePath, exportPath, productionSetName,
                metadataProfileName);

            return ConvertToActionResult(r);
        }
    }
}
