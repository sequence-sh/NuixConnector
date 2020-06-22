using System.Collections.Generic;
using System.Linq;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.Utilities.InstantConsole;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// All nuix processes.
    /// </summary>
    public static class AllProcesses
    {

        /// <summary>
        /// All nuix processes.
        /// </summary>
        public static readonly IReadOnlyCollection<IDocumented> Processes = new List<Process>
        {
            new NuixAddConcordance(),
            new NuixAddItem(),
            new NuixAddToItemSet(),
            new NuixAddToProductionSet(),
            new NuixAnnotateDocumentIdList(),
            new NuixAssertPrintPreviewState(),
            new NuixAssignCustodian(),
            new NuixCountItems(),
            new NuixCreateCase(),
            new NuixCreateIrregularItemsReport(),
            new NuixCreateNRTReport(),
            new NuixCreateReport(),
            new NuixCreateTermList(),
            new NuixDoesCaseExists(),
            new NuixExportConcordance(),
            new NuixExtractEntities(),
            new NuixGeneratePrintPreviews(),
            new NuixGetItemProperties(),
            new NuixImportDocumentIds(),
            new NuixMigrateCase(),
            new NuixPerformOCR(),
            new NuixRemoveFromProductionSet(),
            new NuixReorderProductionSet(),
            new NuixSearchAndTag()
        }.Select(x=> new  YamlObjectWrapper(x.GetType(),new DocumentationCategory("Nuix Processes"))).ToList();
    }
}
