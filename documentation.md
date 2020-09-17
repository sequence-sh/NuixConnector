# Nuix Meta
<a name="GenerateScripts"></a>
## GenerateScripts

Generates ruby scripts for all RubyScriptProcesses in the AppDomain.

|Parameter |Type    |Required|Summary                                     |
|:--------:|:------:|:------:|:------------------------------------------:|
|folderPath|`string`|☑️      |Path to the folder to create the scripts in.|

<a name="RubyScriptProcessUnit"></a>
# Nuix Processes
<a name="NuixAddConcordance"></a>
## NuixAddConcordance

**NuixAddConcordance**

*(Nuix, 7.6, , )*

*(NuixCASE_CREATION, , , )*

*(NuixMETADATA_IMPORT, , , )*

Adds data from a Concordance file to a NUIX case.

<a name="NuixAddItem"></a>
## NuixAddItem

**NuixAddItem**

*(Nuix, 5.0, , )*

*(NuixCASE_CREATION, , , )*

Adds a file or directory to a Nuix Case.

<a name="NuixAddToItemSet"></a>
## NuixAddToItemSet

**NuixAddToItemSet**

*(Nuix, 5.0, , )*

*(NuixANALYSIS, , , )*

Searches a case with a particular search string and adds all items it finds to a particular item set. Will create a new item set if one doesn't already exist.

<a name="NuixAddToProductionSet"></a>
## NuixAddToProductionSet

**NuixAddToProductionSet**

*(Nuix, 7.2, , )*

*(NuixPRODUCTION_SET, , , )*

Searches a case with a particular search string and adds all items it finds to a production set. Will create a new production set if one with the given name does not already exist.

<a name="NuixAnnotateDocumentIdList"></a>
## NuixAnnotateDocumentIdList

**NuixAnnotateDocumentIdList**

*(Nuix, 7.4, , )*

*(NuixPRODUCTION_SET, , , )*

Annotates a document ID list to add production set names to it.

<a name="NuixAssertPrintPreviewState"></a>
## NuixAssertPrintPreviewState

**NuixAssertPrintPreviewState**

*(Nuix, 5.2, , )*

*(NuixPRODUCTION_SET, , , )*

*(NuixANALYSIS, , , )*

Checks the print preview state of the production set.

<a name="NuixAssignCustodian"></a>
## NuixAssignCustodian

**NuixAssignCustodian**

*(Nuix, 5.0, , )*

*(NuixANALYSIS, , , )*

Searches a NUIX case with a particular search string and assigns all files it finds to a particular custodian.

<a name="NuixCountItems"></a>
## NuixCountItems

**NuixCountItems**

*(Nuix, 5.0, , )*

Returns the number of items matching a particular search term

<a name="NuixCreateCase"></a>
## NuixCreateCase

**NuixCreateCase**

*(Nuix, 5.0, , )*

*(NuixCASE_CREATION, , , )*

Creates a new case.

<a name="NuixCreateIrregularItemsReport"></a>
## NuixCreateIrregularItemsReport

**NuixCreateIrregularItemsReport**

*(Nuix, 5.0, , )*

Creates a list of all irregular items in a case. The report is in CSV format. The headers are 'Reason', 'Path' and 'Guid' Reasons include 'NonSearchablePDF','BadExtension','Unrecognised','Unsupported','TextNotIndexed','ImagesNotProcessed','Poisoned','Record','UnrecognisedDeleted','NeedManualExamination', and 'CodeTextFiles' Use this inside a WriteFile process to write it to a file.

<a name="NuixCreateNRTReport"></a>
## NuixCreateNRTReport

**NuixCreateNRTReport**

*(Nuix, 7.4, , )*

*(NuixANALYSIS, , , )*

Creates a report using an NRT file.

<a name="NuixCreateReport"></a>
## NuixCreateReport

**NuixCreateReport**

*(Nuix, 6.2, , )*

*(NuixANALYSIS, , , )*

Creates a report for a Nuix case. The report is in csv format. The headers are 'Custodian', 'Type', 'Value', and 'Count'. The different types are: 'Kind', 'Type', 'Tag', and 'Address'. Use this inside a WriteFile process to write it to a file.

<a name="NuixCreateTermList"></a>
## NuixCreateTermList

**NuixCreateTermList**

*(Nuix, 5.0, , )*

Creates a list of all terms appearing in the case and their frequencies. The report is in CSV format. The headers are 'Term' and 'Count' Use this inside a WriteFile process to write it to a file.

<a name="NuixDoesCaseExists"></a>
## NuixDoesCaseExists

**NuixDoesCaseExists**

*(Nuix, 5.0, , )*

Returns whether or not a case exists.

<a name="NuixExportConcordance"></a>
## NuixExportConcordance

**NuixExportConcordance**

*(Nuix, 7.2, , )*

*(NuixPRODUCTION_SET, , , )*

*(NuixEXPORT_ITEMS, , , )*

Exports Concordance for a particular production set.

<a name="NuixExtractEntities"></a>
## NuixExtractEntities

**NuixExtractEntities**

*(Nuix, 5.0, , )*

Extract Entities from a Nuix Case.

<a name="NuixGeneratePrintPreviews"></a>
## NuixGeneratePrintPreviews

**NuixGeneratePrintPreviews**

*(Nuix, 5.2, , )*

*(NuixPRODUCTION_SET, , , )*

Generates print previews for items in a production set.

<a name="NuixGetItemProperties"></a>
## NuixGetItemProperties

**NuixGetItemProperties**

*(Nuix, 6.2, , )*

A process that the searches a case for items and outputs the values of item properties. The report is in CSV format. The headers are 'Key', 'Value', 'Path' and 'Guid' Use this inside a WriteFile process to write it to a file.

<a name="NuixImportDocumentIds"></a>
## NuixImportDocumentIds

**NuixImportDocumentIds**

*(Nuix, 7.4, , )*

*(NuixPRODUCTION_SET, , , )*

Imports the given document IDs into this production set. Only works if this production set has imported numbering.

<a name="NuixMigrateCase"></a>
## NuixMigrateCase

**NuixMigrateCase**

*(Nuix, 7.2, , )*

Migrates a case to the latest version if necessary.

<a name="NuixPerformOCR"></a>
## NuixPerformOCR

**NuixPerformOCR**

*(Nuix, 7.6, , )*

*(NuixOCR_PROCESSING, , , )*

Performs optical character recognition on files in a NUIX case.

<a name="NuixRemoveFromProductionSet"></a>
## NuixRemoveFromProductionSet

**NuixRemoveFromProductionSet**

*(Nuix, 5.0, , )*

*(NuixPRODUCTION_SET, , , )*

Removes particular items from a Nuix production set.

<a name="NuixReorderProductionSet"></a>
## NuixReorderProductionSet

**NuixReorderProductionSet**

*(Nuix, 5.2, , )*

*(NuixPRODUCTION_SET, , , )*

Reorders and renumbers the items in a production set.

<a name="NuixSearchAndTag"></a>
## NuixSearchAndTag

**NuixSearchAndTag**

*(Nuix, 5.0, , )*

*(NuixANALYSIS, , , )*

Searches a NUIX case with a particular search string and tags all files it finds.

