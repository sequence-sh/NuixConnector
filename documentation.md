<a name="Process"></a>
# General Processes
<a name="ChainLink"></a>
## ChainLink

**The same as the type of the final process in the chain.**

|Parameter|Type                   |Required|Summary|
|:-------:|:---------------------:|:------:|:-----:|
|Process  |[Process](#Process)    |☑️      |       |
|Into     |[ChainLink](#ChainLink)|        |       |
|Inject   |[Injection](#Injection)|☑️      |       |

<a name="Chain"></a>
## Chain

**The same as the type of the final process in the chain.**

|Parameter|Type                   |Required|Summary|
|:-------:|:---------------------:|:------:|:-----:|
|Process  |[Process](#Process)    |☑️      |       |
|Into     |[ChainLink](#ChainLink)|        |       |

<a name="AssertError"></a>
## AssertError

**Unit**

|Parameter|Type               |Required|Summary|
|:-------:|:-----------------:|:------:|:-----:|
|Process  |[Process](#Process)|☑️      |       |

<a name="AssertFalse"></a>
## AssertFalse

**Unit**

|Parameter|Type               |Required|Summary|
|:-------:|:-----------------:|:------:|:-----:|
|ResultOf |[Process](#Process)|        |       |

<a name="AssertTrue"></a>
## AssertTrue

**Unit**

|Parameter|Type               |Required|Summary|
|:-------:|:-----------------:|:------:|:-----:|
|ResultOf |[Process](#Process)|        |       |

<a name="CheckNumber"></a>
## CheckNumber

**Boolean**

|Parameter|Type               |Required|Summary|
|:-------:|:-----------------:|:------:|:-----:|
|Minimum  |`int`?             |        |       |
|Maximum  |`int`?             |        |       |
|Check    |[Process](#Process)|        |       |

<a name="Conditional"></a>
## Conditional

**Returns the same type as the 'Then' and 'Else' processes. Returns void if there is no Else process.**

|Parameter|Type               |Required|Summary|
|:-------:|:-----------------:|:------:|:-----:|
|If       |[Process](#Process)|☑️      |       |
|Then     |[Process](#Process)|☑️      |       |
|Else     |[Process](#Process)|        |       |

<a name="CreateDirectory"></a>
## CreateDirectory

**Unit**

|Parameter|Type    |Required|Summary|
|:-------:|:------:|:------:|:-----:|
|Path     |`string`|☑️      |       |

<a name="DeleteItem"></a>
## DeleteItem

**Unit**

|Parameter|Type    |Required|Summary|
|:-------:|:------:|:------:|:-----:|
|Path     |`string`|☑️      |       |

<a name="DoesFileContain"></a>
## DoesFileContain

**Boolean**

|Parameter       |Type    |Required|Summary|
|:--------------:|:------:|:------:|:-----:|
|ExpectedContents|`string`|☑️      |       |
|FilePath        |`string`|☑️      |       |

<a name="Loop"></a>
## Loop

**Unit**

|Parameter|Type                       |Required|Summary|
|:-------:|:-------------------------:|:------:|:-----:|
|For      |[Enumeration](#Enumeration)|☑️      |       |
|Do       |[Process](#Process)        |☑️      |       |

<a name="RunExternalProcess"></a>
## RunExternalProcess

**Unit**

|Parameter  |Type          |Required|Summary|Default Value                                   |
|:---------:|:------------:|:------:|:-----:|:----------------------------------------------:|
|ProcessPath|`string`      |☑️      |       |                                                |
|Arguments  |List<`string`>|        |       |System.Collections.Generic.List`1[System.String]|

<a name="Sequence"></a>
## Sequence

**Unit**

|Parameter|Type                     |Required|Summary|
|:-------:|:-----------------------:|:------:|:-----:|
|Steps    |List<[Process](#Process)>|☑️      |       |

<a name="Unzip"></a>
## Unzip

**Unit**

|Parameter           |Type    |Required|Summary|Default Value|
|:------------------:|:------:|:------:|:-----:|:-----------:|
|ArchiveFilePath     |`string`|☑️      |       |             |
|DestinationDirectory|`string`|☑️      |       |             |
|OverwriteFiles      |`bool`  |        |       |False        |

<a name="WriteFile"></a>
## WriteFile

**Unit**

|Parameter|Type               |Required|Summary|
|:-------:|:-----------------:|:------:|:-----:|
|Text     |[Process](#Process)|        |       |

<a name="Enumeration"></a>
# Enumerations
<a name="List"></a>
## List

|Parameter|Type                         |Required|Summary|
|:-------:|:---------------------------:|:------:|:-----:|
|Members  |List<`string`>               |☑️      |       |
|Inject   |List<[Injection](#Injection)>|☑️      |       |

<a name="CSV"></a>
## CSV

|Parameter                |Type                                     |Required|Summary|Default Value|
|:-----------------------:|:---------------------------------------:|:------:|:-----:|:-----------:|
|ColumnInjections         |List<[ColumnInjection](#ColumnInjection)>|☑️      |       |             |
|CommentToken             |`string`                                 |        |       |             |
|CSVFilePath              |`string`                                 |        |       |             |
|CSVProcess               |[Process](#Process)                      |        |       |             |
|CSVText                  |`string`                                 |        |       |             |
|Delimiter                |`string`                                 |        |       |,            |
|Distinct                 |`bool`                                   |        |       |False        |
|HasFieldsEnclosedInQuotes|`bool`                                   |        |       |False        |

<a name="Directory"></a>
## Directory

|Parameter|Type                         |Required|Summary|
|:-------:|:---------------------------:|:------:|:-----:|
|Path     |`string`                     |☑️      |       |
|Injection|List<[Injection](#Injection)>|☑️      |       |

<a name="Injection"></a>
# Injections
<a name="Injection"></a>
## Injection

|Parameter|Type    |Required|Summary|Default Value                         |
|:-------:|:------:|:------:|:-----:|:------------------------------------:|
|Property |`string`|☑️      |       |                                      |
|Regex    |`string`|        |       |The entire value will be injected.    |
|Template |`string`|        |       |The value will be injected on its own.|

# Nuix Processes
<a name="NuixAddConcordance"></a>
## NuixAddConcordance

**Unit**

*Requires Nuix Version 7.6*

*Requires Nuix Feature 'CASE_CREATION'*

*Requires Nuix Feature 'METADATA_IMPORT'*

Adds data from a Concordance file to a NUIX case.

|Parameter             |Type    |Required|Summary                                           |
|:--------------------:|:------:|:------:|:------------------------------------------------:|
|ConcordanceProfileName|`string`|☑️      |The name of the concordance profile to use.       |
|ConcordanceDateFormat |`string`|☑️      |The concordance date format to use.               |
|FilePath              |`string`|☑️      |The path of the concordance file to import.       |
|Custodian             |`string`|☑️      |The name of the custodian to assign the folder to.|
|Description           |`string`|        |A description to add to the folder.               |
|FolderName            |`string`|☑️      |The name of the folder to create.                 |
|CasePath              |`string`|☑️      |The path to the case to import into.              |

<a name="NuixAddItem"></a>
## NuixAddItem

**Unit**

*Requires Nuix Version 5.0*

*Requires Nuix Feature 'CASE_CREATION'*

Adds a file or directory to a Nuix Case.

|Parameter            |Type    |Required|Summary                                                       |Default Value                               |Requirements|
|:-------------------:|:------:|:------:|:------------------------------------------------------------:|:------------------------------------------:|:----------:|
|Path                 |`string`|☑️      |The path of the file or directory to add to the case.         |                                            |            |
|Custodian            |`string`|☑️      |The custodian to assign to the new folder.                    |                                            |            |
|Description          |`string`|        |The description of the new folder.                            |                                            |            |
|FolderName           |`string`|☑️      |The name of the folder to create.                             |                                            |            |
|CasePath             |`string`|☑️      |The path to the case.                                         |                                            |            |
|PasswordFilePath     |`string`|☑️      |The path of a file containing passwords to use for decryption.|                                            |Nuix 7.6    |
|ProcessingProfileName|`string`|        |The name of the Processing profile to use.                    |The default processing profile will be used.|Nuix 7.6    |
|ProcessingProfilePath|`string`|        |The path to the Processing profile to use                     |The default processing profile will be used.|Nuix 7.6    |

<a name="NuixAddToItemSet"></a>
## NuixAddToItemSet

**Unit**

*Requires Nuix Version 5.0*

*Requires Nuix Feature 'ANALYSIS'*

Searches a case with a particular search string and adds all items it finds to a particular item set. Will create a new item set if one doesn't already exist.

|Parameter           |Type                                         |Required|Summary                                                                                                                                                                                      |Default Value|
|:------------------:|:-------------------------------------------:|:------:|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:|:-----------:|
|ItemSetName         |`string`                                     |☑️      |The item set to add results to. Will be created if it doesn't already exist.                                                                                                                 |             |
|SearchTerm          |`string`                                     |☑️      |The term to search for.                                                                                                                                                                      |             |
|CasePath            |`string`                                     |☑️      |The path of the case to search.                                                                                                                                                              |             |
|ItemSetDeduplication|[ItemSetDeduplication](#ItemSetDeduplication)|        |The means of deduplicating items by key and prioritizing originals in a tie-break.                                                                                                           |Default      |
|ItemSetDescription  |`string`                                     |        |The description of the item set.                                                                                                                                                             |             |
|Order               |`string`                                     |        |How to order the items to be added to the item set.                                                                                                                                          |             |
|DeduplicateBy       |[DeduplicateBy](#DeduplicateBy)              |        |Whether to deduplicate as a family or individual.                                                                                                                                            |Individual   |
|Limit               |`int`?                                       |        |The maximum number of items to add to the item set.                                                                                                                                          |             |
|CustodianRanking    |List<`string`>                               |        |A list of custodian names ordered from highest ranked to lowest ranked. If this parameter is present and the deduplication parameter has not been specified, MD5 Ranked Custodian is assumed.|             |

<a name="NuixAddToProductionSet"></a>
## NuixAddToProductionSet

**Unit**

*Requires Nuix Version 7.2*

*Requires Nuix Feature 'PRODUCTION_SET'*

Searches a case with a particular search string and adds all items it finds to a production set. Will create a new production set if one with the given name does not already exist.

|Parameter            |Type    |Required|Summary                                                                                         |Default Value                               |Requirements|
|:-------------------:|:------:|:------:|:----------------------------------------------------------------------------------------------:|:------------------------------------------:|:----------:|
|ProductionSetName    |`string`|☑️      |The production set to add results to. Will be created if it doesn't already exist               |                                            |            |
|SearchTerm           |`string`|☑️      |The term to search for                                                                          |                                            |            |
|CasePath             |`string`|☑️      |The path of the case to search                                                                  |                                            |            |
|Description          |`string`|        |Description of the production set.                                                              |                                            |            |
|Order                |`string`|        |How to order the items to be added to the production set.                                       |                                            |            |
|Limit                |`int`?  |        |The maximum number of items to add to the production set.                                       |                                            |            |
|ProductionProfileName|`string`|        |The name of the Production profile to use. Either this or the ProductionProfilePath must be set |The default processing profile will be used.|Nuix 7.2    |
|ProductionProfilePath|`string`|        |The path to the Production profile to use. Either this or the ProductionProfileName must be set.|The default processing profile will be used.|Nuix 7.6    |

<a name="NuixAnnotateDocumentIdList"></a>
## NuixAnnotateDocumentIdList

**Unit**

*Requires Nuix Version 7.4*

*Requires Nuix Feature 'PRODUCTION_SET'*

Annotates a document ID list to add production set names to it.

|Parameter        |Type    |Required|Summary                                         |
|:---------------:|:------:|:------:|:----------------------------------------------:|
|ProductionSetName|`string`|☑️      |The production set to get names from.           |
|CasePath         |`string`|☑️      |The path to the case.                           |
|DataPath         |`string`|☑️      |Specifies the file path of the document ID list.|

<a name="NuixAssertPrintPreviewState"></a>
## NuixAssertPrintPreviewState

**Unit**

*Requires Nuix Version 5.2*

*Requires Nuix Feature 'ANALYSIS'*

*Requires Nuix Feature 'PRODUCTION_SET'*

Checks the print preview state of the production set.

|Parameter        |Type                                   |Required|Summary                                                |Default Value|
|:---------------:|:-------------------------------------:|:------:|:-----------------------------------------------------:|:-----------:|
|ExpectedState    |[PrintPreviewState](#PrintPreviewState)|        |The expected print preview state of the production set;|All          |
|ProductionSetName|`string`                               |☑️      |The production set to reorder.                         |             |
|CasePath         |`string`                               |☑️      |The path to the case.                                  |             |

<a name="NuixAssignCustodian"></a>
## NuixAssignCustodian

**Unit**

*Requires Nuix Version 5.0*

*Requires Nuix Feature 'ANALYSIS'*

Searches a NUIX case with a particular search string and assigns all files it finds to a particular custodian.

|Parameter |Type    |Required|Summary                                  |
|:--------:|:------:|:------:|:---------------------------------------:|
|Custodian |`string`|☑️      |The custodian to assign to found results.|
|SearchTerm|`string`|☑️      |The term to search for.                  |
|CasePath  |`string`|☑️      |The path to the case.                    |

<a name="NuixCountItems"></a>
## NuixCountItems

**Unit**

*Requires Nuix Version 5.0*

Returns the number of items matching a particular search term

|Parameter |Type    |Required|Summary                  |
|:--------:|:------:|:------:|:-----------------------:|
|CasePath  |`string`|☑️      |The path to the case.    |
|SearchTerm|`string`|☑️      |The search term to count.|

<a name="NuixCreateCase"></a>
## NuixCreateCase

**Unit**

*Requires Nuix Version 5.0*

*Requires Nuix Feature 'CASE_CREATION'*

Creates a new case.

|Parameter   |Type    |Required|Summary                                      |
|:----------:|:------:|:------:|:-------------------------------------------:|
|CaseName    |`string`|☑️      |The name of the case to create.              |
|CasePath    |`string`|☑️      |The path to the folder to create the case in.|
|Investigator|`string`|☑️      |Name of the investigator.                    |
|Description |`string`|        |Description of the case.                     |

<a name="NuixCreateIrregularItemsReport"></a>
## NuixCreateIrregularItemsReport

**Unit**

*Requires Nuix Version 5.0*

Creates a list of all irregular items in a case. The report is in CSV format. The headers are 'Reason', 'Path' and 'Guid' Reasons include 'NonSearchablePDF','BadExtension','Unrecognised','Unsupported','TextNotIndexed','ImagesNotProcessed','Poisoned','Record','UnrecognisedDeleted','NeedManualExamination', and 'CodeTextFiles' Use this inside a WriteFile process to write it to a file.

|Parameter|Type    |Required|Summary              |
|:-------:|:------:|:------:|:-------------------:|
|CasePath |`string`|☑️      |The path to the case.|

<a name="NuixCreateNRTReport"></a>
## NuixCreateNRTReport

**Unit**

*Requires Nuix Version 7.4*

*Requires Nuix Feature 'ANALYSIS'*

Creates a report using an NRT file.

|Parameter        |Type    |Required|Summary                                                        |
|:---------------:|:------:|:------:|:-------------------------------------------------------------:|
|CasePath         |`string`|☑️      |The path to the case.                                          |
|NRTPath          |`string`|☑️      |The NRT file path.                                             |
|OutputFormat     |`string`|☑️      |The format of the report file that will be created.            |
|LocalResourcesURL|`string`|☑️      |The path to the local resources folder. To load the logo's etc.|
|OutputPath       |`string`|☑️      |The path to output the file at.                                |

<a name="NuixCreateReport"></a>
## NuixCreateReport

**Unit**

*Requires Nuix Version 6.2*

*Requires Nuix Feature 'ANALYSIS'*

Creates a report for a Nuix case. The report is in csv format. The headers are 'Custodian', 'Type', 'Value', and 'Count'. The different types are: 'Kind', 'Type', 'Tag', and 'Address'. Use this inside a WriteFile process to write it to a file.

|Parameter|Type    |Required|Summary              |
|:-------:|:------:|:------:|:-------------------:|
|CasePath |`string`|☑️      |The path to the case.|

<a name="NuixCreateTermList"></a>
## NuixCreateTermList

**Unit**

*Requires Nuix Version 5.0*

Creates a list of all terms appearing in the case and their frequencies. The report is in CSV format. The headers are 'Term' and 'Count' Use this inside a WriteFile process to write it to a file.

|Parameter|Type    |Required|Summary              |
|:-------:|:------:|:------:|:-------------------:|
|CasePath |`string`|☑️      |The path to the case.|

<a name="NuixDoesCaseExists"></a>
## NuixDoesCaseExists

**Unit**

*Requires Nuix Version 5.0*

Returns whether or not a case exists.

|Parameter|Type    |Required|Summary              |
|:-------:|:------:|:------:|:-------------------:|
|CasePath |`string`|☑️      |The path to the case.|

<a name="NuixExportConcordance"></a>
## NuixExportConcordance

**Unit**

*Requires Nuix Version 7.2*

*Requires Nuix Feature 'EXPORT_ITEMS'*

*Requires Nuix Feature 'PRODUCTION_SET'*

Exports Concordance for a particular production set.

|Parameter        |Type    |Required|Summary                                  |
|:---------------:|:------:|:------:|:---------------------------------------:|
|ProductionSetName|`string`|☑️      |The name of the production set to export.|
|ExportPath       |`string`|☑️      |Where to export the Concordance to.      |
|CasePath         |`string`|☑️      |The path to the case.                    |

<a name="NuixExtractEntities"></a>
## NuixExtractEntities

**Unit**

*Requires Nuix Version 5.0*

Extract Entities from a Nuix Case.

|Parameter   |Type    |Required|Summary                                           |
|:----------:|:------:|:------:|:------------------------------------------------:|
|OutputFolder|`string`|☑️      |The path to the folder to put the output files in.|
|CasePath    |`string`|☑️      |The path to the case.                             |

<a name="NuixGeneratePrintPreviews"></a>
## NuixGeneratePrintPreviews

**Unit**

*Requires Nuix Version 5.2*

*Requires Nuix Feature 'PRODUCTION_SET'*

Generates print previews for items in a production set.

|Parameter        |Type    |Required|Summary                                           |
|:---------------:|:------:|:------:|:------------------------------------------------:|
|ProductionSetName|`string`|☑️      |The production set to generate print previews for.|
|CasePath         |`string`|☑️      |The path to the case.                             |

<a name="NuixGetItemProperties"></a>
## NuixGetItemProperties

**Unit**

*Requires Nuix Version 6.2*

A process that the searches a case for items and outputs the values of item properties. The report is in CSV format. The headers are 'Key', 'Value', 'Path' and 'Guid' Use this inside a WriteFile process to write it to a file.

|Parameter    |Type    |Required|Summary                                                                                                                                                     |
|:-----------:|:------:|:------:|:----------------------------------------------------------------------------------------------------------------------------------------------------------:|
|CasePath     |`string`|☑️      |The path to the case.                                                                                                                                       |
|SearchTerm   |`string`|☑️      |The term to search for.                                                                                                                                     |
|PropertyRegex|`string`|☑️      |The regex to search the property for.                                                                                                                       |
|ValueRegex   |`string`|        |An optional regex to check the value. If this is set, only values which match this regex will be returned, and only the contents of the first capture group.|

<a name="NuixImportDocumentIds"></a>
## NuixImportDocumentIds

**Unit**

*Requires Nuix Version 7.4*

*Requires Nuix Feature 'PRODUCTION_SET'*

Imports the given document IDs into this production set. Only works if this production set has imported numbering.

|Parameter                    |Type    |Required|Summary                                                                                |Default Value|
|:---------------------------:|:------:|:------:|:-------------------------------------------------------------------------------------:|:-----------:|
|ProductionSetName            |`string`|☑️      |The production set to add results to.                                                  |             |
|CasePath                     |`string`|☑️      |The path to the case.                                                                  |             |
|AreSourceProductionSetsInData|`bool`  |        |Specifies that the source production set name(s) are contained in the document ID list.|False        |
|DataPath                     |`string`|☑️      |Specifies the file path of the document ID list.                                       |             |

<a name="NuixMigrateCase"></a>
## NuixMigrateCase

**Unit**

*Requires Nuix Version 5.0*

Migrates a case to the latest version if necessary.

|Parameter|Type    |Required|Summary              |
|:-------:|:------:|:------:|:-------------------:|
|CasePath |`string`|☑️      |The path to the case.|

<a name="NuixPerformOCR"></a>
## NuixPerformOCR

**Unit**

*Requires Nuix Version 7.6*

*Requires Nuix Feature 'OCR_PROCESSING'*

Performs optical character recognition on files in a NUIX case.

|Parameter     |Type    |Required|Summary                                                                                   |Default Value                                                                                                                                      |Requirements|
|:------------:|:------:|:------:|:----------------------------------------------------------------------------------------:|:-------------------------------------------------------------------------------------------------------------------------------------------------:|:----------:|
|CasePath      |`string`|☑️      |The path to the case.                                                                     |                                                                                                                                                   |            |
|SearchTerm    |`string`|        |The term to use for searching for files to OCR.                                           |NOT flag:encrypted AND ((mime-type:application/pdf AND NOT content:\*) OR (mime-type:image/\* AND ( flag:text_not_indexed OR content:( NOT \* ) )))|            |
|OCRProfileName|`string`|        |The name of the OCR profile to use. This cannot be set at the same time as OCRProfilePath.|The default profile will be used.                                                                                                                  |            |
|OCRProfilePath|`string`|        |Path to the OCR profile to use. This cannot be set at the same times as OCRProfileName.   |The default profile will be used.                                                                                                                  |Nuix 7.6    |

<a name="NuixRemoveFromProductionSet"></a>
## NuixRemoveFromProductionSet

**Unit**

*Requires Nuix Version 5.0*

*Requires Nuix Feature 'PRODUCTION_SET'*

Removes particular items from a Nuix production set.

|Parameter        |Type    |Required|Summary                                                   |Default Value             |
|:---------------:|:------:|:------:|:--------------------------------------------------------:|:------------------------:|
|ProductionSetName|`string`|☑️      |The production set to remove results from.                |                          |
|SearchTerm       |`string`|        |The search term to use for choosing which items to remove.|All items will be removed.|
|CasePath         |`string`|☑️      |The path to the case.                                     |                          |

<a name="NuixReorderProductionSet"></a>
## NuixReorderProductionSet

**Unit**

*Requires Nuix Version 5.2*

*Requires Nuix Feature 'PRODUCTION_SET'*

Reorders and renumbers the items in a production set.

|Parameter        |Type                                             |Required|Summary                                            |Default Value|
|:---------------:|:-----------------------------------------------:|:------:|:-------------------------------------------------:|:-----------:|
|ProductionSetName|`string`                                         |☑️      |The production set to reorder.                     |             |
|CasePath         |`string`                                         |☑️      |The path to the case.                              |             |
|SortOrder        |[ProductionSetSortOrder](#ProductionSetSortOrder)|        |The method of sorting items during the renumbering.|Position     |

<a name="NuixSearchAndTag"></a>
## NuixSearchAndTag

**Unit**

*Requires Nuix Version 5.0*

*Requires Nuix Feature 'ANALYSIS'*

Searches a NUIX case with a particular search string and tags all files it finds.

|Parameter |Type    |Required|Summary                            |
|:--------:|:------:|:------:|:---------------------------------:|
|Tag       |`string`|☑️      |The tag to assign to found results.|
|SearchTerm|`string`|☑️      |The term to search for.            |
|CasePath  |`string`|☑️      |The path to the case.              |

# Enums
<a name="DeduplicateBy"></a>
## DeduplicateBy
Whether to deduplicate as a family or individual

|Name      |Summary                                                                                                                                                                                                                                                                           |
|:--------:|:--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:|
|Individual|Deduplication by individual treats each item as an individual and an attachment or embedded item has the same priority for deduplication as a loose file.                                                                                                                         |
|Family    |Items can be treated as a family where only the top-level item of a family is deduplicated and the descendants are classified as original or duplicate with their family as a group. The top-level item does not have to be in the set for its descendants to classified this way.|

<a name="ItemSetDeduplication"></a>
## ItemSetDeduplication
The means of deduplicating items by key and prioritizing originals in a tie-break. 

|Name              |Summary                                                                                                                                                                                                 |
|:----------------:|:------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:|
|Default           |MD5RankedCustodian if a custodian ranking is given, MD5 otherwise                                                                                                                                       |
|MD5               |MD5 results in items with the same MD5 hash being identical. Tie breaking is by highest path order.                                                                                                     |
|MD5PerCustodian   |MD5 Per Custodian results in items with the same MD5 hash and custodian being identical. Tie breaking is by highest path order.                                                                         |
|MD5RankedCustodian|MD5 Ranked Custodian results in items with MD5 hash being identical. Tie breaking is by the item with the highest ranked custodian or highest path order if custodian ranking is equal.                 |
|Scripted          |Scripted results in items being deduplicated based on an expression defined by the script and passed to ItemSet.addItems. It is an error to add items to this Item Set without supplying the expression.|
|None              |None results in all items being added to the set without deduplication.                                                                                                                                 |

<a name="PrintPreviewState"></a>
## PrintPreviewState
The state of print previews in the production set.

|Name|Summary                             |
|:--:|:----------------------------------:|
|All |All documents have a print preview. |
|Some|Some documents have a print preview.|
|None|No documents have a print preview.  |

<a name="ProductionSetSortOrder"></a>
## ProductionSetSortOrder
Selects the method of sorting items during production set sort ordering

|Name                      |Summary                                                                                   |
|:------------------------:|:----------------------------------------------------------------------------------------:|
|Position                  |Default sort order (fastest). Sorts as documented in ItemSorter.sortItemsByPosition(List).|
|TopLevelItemDate          |Sorts as documented in ItemSorter.sortItemsByTopLevelItemDate(List).                      |
|TopLevelItemDateDescending|Sorts as documented in ItemSorter.sortItemsByTopLevelItemDateDescending(List).            |
|DocumentId                |Sorts items based on their document IDs for the production set.                           |

