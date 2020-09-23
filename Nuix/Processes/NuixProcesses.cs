//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using Reductech.EDR.Connectors.Nuix.Processes.Meta;
//using Reductech.EDR.Processes;
//using Reductech.EDR.Processes.Internal;
//using Reductech.EDR.Processes.Serialization;
//using Reductech.Utilities.InstantConsole;

//namespace Reductech.EDR.Connectors.Nuix.Processes
//{
//    /// <summary>
//    /// All nuix processes.
//    /// </summary>
//    public static class NuixProcesses
//    {

//        /// <summary>
//        /// Gets all the nuix processes
//        /// </summary>
//        /// <param name="nuixProcessSettings"></param>
//        /// <returns></returns>
//        public static IReadOnlyCollection<YamlObjectWrapper> GetProcesses(INuixProcessSettings nuixProcessSettings)
//        {
//            var r = AllProcesses.Select(x => new ProcessWrapper<INuixProcessSettings>(x.GetType(), nuixProcessSettings,
//                new DocumentationCategory("Nuix Processes"))).ToList();

//            return r;
//        }

//        /// <summary>
//        /// All nuix processes.
//        /// </summary>
//        private static readonly IReadOnlyCollection<RubyScriptProcessUnit> AllProcesses = new List<RubyScriptProcessUnit>
//        {
//            new NuixAddConcordance(),
//            new NuixAddItem(),
//            new NuixAddToItemSet(),
//            new NuixAddToProductionSet(),
//            new NuixAnnotateDocumentIdList(),
//            new NuixAssertPrintPreviewState(),
//            new NuixAssignCustodian(),
//            new NuixCountItems(),
//            new NuixCreateCase(),
//            new NuixCreateIrregularItemsReport(),
//            new NuixCreateNRTReport(),
//            new NuixCreateReport(),
//            new NuixCreateTermList(),
//            new NuixDoesCaseExists(),
//            new NuixExportConcordance(),
//            new NuixExtractEntities(),
//            new NuixGeneratePrintPreviews(),
//            new NuixGetItemProperties(),
//            new NuixImportDocumentIds(),
//            new NuixMigrateCase(),
//            new NuixPerformOCR(),
//            new NuixRemoveFromProductionSet(),
//            new NuixReorderProductionSet(),
//            new NuixSearchAndTag()
//        }.ToList();
//    }
//}
