## RunProcessFromYamlString

Run process defined in yaml

|Parameter |Type    |Required|Default|Summary                      |
|:--------:|:------:|:------:|:-----:|:---------------------------:|
|yamlString|`string`|☑️      |       |Yaml representing the process|
## RunProcessFromYaml

Run process defined in yaml found at a particular path

|Parameter|Type    |Required|Default|Summary         |
|:-------:|:------:|:------:|:-----:|:--------------:|
|yamlPath |`string`|☑️      |       |Path to the yaml|
## NuixAddConcordance

Adds data from a Concordance file to a NUIX case.

|Parameter             |Type    |Required|Default|Summary                                           |
|:--------------------:|:------:|:------:|:-----:|:------------------------------------------------:|
|ConcordanceProfileName|`string`|☑️      |       |The name of the concordance profile to use.       |
|ConcordanceDateFormat |`string`|☑️      |       |The concordance date format to use.               |
|FilePath              |`string`|☑️      |       |The path of the concordance file to import.       |
|Custodian             |`string`|☑️      |       |The name of the custodian to assign the folder to.|
|Description           |`string`|☑️      |       |A description to add to the folder.               |
|FolderName            |`string`|☑️      |       |The name of the folder to create.                 |
|CasePath              |`string`|☑️      |       |The name of the case to import into.              |
## NuixAddFile

Adds a file or folder to a Nuix Case

|Parameter            |Type    |Required|Default|Summary                                                                                         |
|:-------------------:|:------:|:------:|:-----:|:----------------------------------------------------------------------------------------------:|
|FilePath             |`string`|☑️      |       |The path of the file or folder to add to the case.                                              |
|Custodian            |`string`|☑️      |       |The custodian to assign to the new folder.                                                      |
|Description          |`string`|☑️      |       |The description of the new folder.                                                              |
|FolderName           |`string`|☑️      |       |The name of the folder to create.                                                               |
|CasePath             |`string`|☑️      |       |The path to the case.                                                                           |
|ProcessingProfileName|`string`|        |       |The name of the processing profile to use. If null, the default processing profile will be used.|
## NuixAddToItemSet

Searches a case with a particular search string and adds all items it finds to a particular item set. Will create a new item set if one doesn't already exist.

|Parameter           |Type                  |Required|Default   |Summary                                                                                                                                                                                      |
|:------------------:|:--------------------:|:------:|:--------:|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:|
|ItemSetName         |`string`              |☑️      |          |The production set to add results to. Will be created if it doesn't already exist                                                                                                            |
|SearchTerm          |`string`              |☑️      |          |The term to search for                                                                                                                                                                       |
|CasePath            |`string`              |☑️      |          |The path of the case to search                                                                                                                                                               |
|ItemSetDeduplication|`ItemSetDeduplication`|        |Default   |The means of deduplicating items by key and prioritizing originals in a tie-break.                                                                                                           |
|ItemSetDescription  |`string`              |        |          |The description of the item set as a string.                                                                                                                                                 |
|DeduplicateBy       |`DeduplicateBy`       |        |Individual|Whether to deduplicate as a family or individual                                                                                                                                             |
|CustodianRanking    |`List<string>`        |        |          |A list of custodian names ordered from highest ranked to lowest ranked. If this parameter is present and the deduplication parameter has not been specified, MD5 Ranked Custodian is assumed.|
## NuixAddToProductionSet

Searches a case with a particular search string and adds all items it finds to a production set. Will create a new production set if one with the given name does not already exist.

|Parameter        |Type    |Required|Default|Summary                                                                                                                        |
|:---------------:|:------:|:------:|:-----:|:-----------------------------------------------------------------------------------------------------------------------------:|
|ProductionSetName|`string`|☑️      |       |The production set to add results to. Will be created if it doesn't already exist                                              |
|SearchTerm       |`string`|☑️      |       |The term to search for                                                                                                         |
|CasePath         |`string`|☑️      |       |The path of the case to search                                                                                                 |
|Description      |`string`|☑️      |       |Description of the production set                                                                                              |
|Order            |`string`|        |       |How to order the items to be added to the production set. e.g. "name ASC", "item-date DESC",  or "name ASC, item-date DESC" etc|
|Limit            |`int?`  |        |       |The maximum number of items to add to the production set.                                                                      |
## NuixReorderProductionSet

Renumbers the items in the production set.

|Parameter        |Type                    |Required|Default |Summary                                                                          |
|:---------------:|:----------------------:|:------:|:------:|:-------------------------------------------------------------------------------:|
|ProductionSetName|`string`                |☑️      |        |The production set to add results to. Will be created if it doesn't already exist|
|CasePath         |`string`                |☑️      |        |The path of the case to search                                                   |
|SortOrder        |`ProductionSetSortOrder`|☑️      |Position|Selects the method of sorting items during the renumber                          |
## NuixAnnotateDocumentIdList

Annotates a document ID list to add production set names to it.

|Parameter        |Type    |Required|Default|Summary                                                                          |
|:---------------:|:------:|:------:|:-----:|:-------------------------------------------------------------------------------:|
|ProductionSetName|`string`|☑️      |       |The production set to add results to. Will be created if it doesn't already exist|
|CasePath         |`string`|☑️      |       |The path of the case to search                                                   |
|DataPath         |`string`|☑️      |       |Specifies the file path of the document ID list.                                 |
## NuixCreateCase

Creates a new case

|Parameter   |Type    |Required|Default|Summary                                      |
|:----------:|:------:|:------:|:-----:|:-------------------------------------------:|
|CaseName    |`string`|☑️      |       |The name of the case to create.              |
|CasePath    |`string`|☑️      |       |The path to the folder to create the case in.|
|Investigator|`string`|☑️      |       |Name of the investigator.                    |
|Description |`string`|☑️      |       |Description of the case.                     |
## NuixCreateIrregularItemsReport

Creates a report detailing the irregular items in a case.

|Parameter   |Type    |Required|Default|Summary                                          |
|:----------:|:------:|:------:|:-----:|:-----------------------------------------------:|
|CasePath    |`string`|☑️      |       |The path to the folder to create the case in.    |
|OutputFolder|`string`|☑️      |       |The path to the folder to put the output files in|
## NuixCreateNRTReport

Creates a report using an NRT file.

|Parameter        |Type    |Required|Default|Summary                                                                                                                                      |
|:---------------:|:------:|:------:|:-----:|:-------------------------------------------------------------------------------------------------------------------------------------------:|
|CasePath         |`string`|☑️      |       |The case folder path.                                                                                                                        |
|NRTPath          |`string`|☑️      |       |The NRT file path                                                                                                                            |
|OutputFormat     |`string`|☑️      |       |The format of the report file that will be created. e.g. PDF or HTML                                                                         |
|LocalResourcesURL|`string`|☑️      |       |The path to the local resources folder. To load the logo's etc. e.g. C:\Program Files\Nuix\Nuix 8.4\user-data\Reports\Case Summary\Resources\|
|OutputPath       |`string`|☑️      |       |The path to output the file at.  e.g. C:/Temp/report.pdf                                                                                     |
## NuixCreateReport

Creates a report for a Nuix case.

|Parameter   |Type    |Required|Default|Summary                                          |
|:----------:|:------:|:------:|:-----:|:-----------------------------------------------:|
|CasePath    |`string`|☑️      |       |The case folder path.                            |
|OutputFolder|`string`|☑️      |       |The path to the folder to put the output files in|
## NuixCreateTermList

Creates a list of all terms appearing in the case and their frequencies.

|Parameter   |Type    |Required|Default|Summary                                          |
|:----------:|:------:|:------:|:-----:|:-----------------------------------------------:|
|CasePath    |`string`|☑️      |       |The path of the case to examine.                 |
|OutputFolder|`string`|☑️      |       |The path to the folder to put the output files in|
## NuixExportConcordance

Exports Concordance for a particular production set.

|Parameter          |Type    |Required|Default|Summary                                                                              |
|:-----------------:|:------:|:------:|:-----:|:-----------------------------------------------------------------------------------:|
|MetadataProfileName|`string`|        |       |The name of the metadata profile to use. Will use the Default profile if this is null|
|ProductionSetName  |`string`|☑️      |       |The name of the production set to export.                                            |
|ExportPath         |`string`|☑️      |       |Where to export the Concordance to.                                                  |
|CasePath           |`string`|☑️      |       |The path of the case to export Concordance from.                                     |
## NuixExtractEntities

Extract Entities from a Nuix Case

|Parameter   |Type    |Required|Default|Summary                                          |
|:----------:|:------:|:------:|:-----:|:-----------------------------------------------:|
|CasePath    |`string`|☑️      |       |The path to the folder to create the case in     |
|OutputFolder|`string`|☑️      |       |The path to the folder to put the output files in|
## NuixGeneratePrintPreviews

Generates print previews for items in the production set

|Parameter        |Type    |Required|Default|Summary                                          |
|:---------------:|:------:|:------:|:-----:|:-----------------------------------------------:|
|ProductionSetName|`string`|☑️      |       |The production set to generate print previews for|
|CasePath         |`string`|☑️      |       |The path of the case to search                   |
## NuixGetParticularProperties

A process that reads searches a case and outputs to a file the values of particular properties of the results

|Parameter     |Type    |Required|Default|Summary                                          |
|:------------:|:------:|:------:|:-----:|:-----------------------------------------------:|
|CasePath      |`string`|☑️      |       |The path of the case to examine.                 |
|SearchTerm    |`string`|☑️      |       |The term to search for                           |
|PropertyRegex |`string`|☑️      |       |The term to search for                           |
|OutputFilePath|`string`|☑️      |       |The name of the file to write the results to     |
|OutputFolder  |`string`|☑️      |       |The path to the folder to put the output files in|
## NuixImportDocumentIds

Imports the given document IDs into this production set. Only works if this production set has imported numbering.

|Parameter                    |Type    |Required|Default|Summary                                                                                |
|:---------------------------:|:------:|:------:|:-----:|:-------------------------------------------------------------------------------------:|
|ProductionSetName            |`string`|☑️      |       |The production set to add results to. Will be created if it doesn't already exist      |
|CasePath                     |`string`|☑️      |       |The path of the case to search                                                         |
|AreSourceProductionSetsInData|`bool`  |☑️      |False  |Specifies that the source production set name(s) are contained in the document ID list.|
|DataPath                     |`string`|☑️      |       |Specifies the file path of the document ID list.                                       |
## NuixMigrateCase

Migrates a case to the latest version if necessary

|Parameter|Type    |Required|Default|Summary                    |
|:-------:|:------:|:------:|:-----:|:-------------------------:|
|CasePath |`string`|☑️      |       |The path to the case folder|
## NuixPerformOCR

Performs optical character recognition on files which need it in a NUIX case.

|Parameter     |Type    |Required|Default|Summary                                                                              |
|:------------:|:------:|:------:|:-----:|:-----------------------------------------------------------------------------------:|
|CasePath      |`string`|☑️      |       |The path to the case                                                                 |
|OCRProfileName|`string`|        |       |The name of the OCR profile to use. If not provided, the default profile will be used|
## NuixRemoveFromProductionSet

A process that removes particular items from a Nuix production set

|Parameter        |Type    |Required|Default|Summary                                                                                       |
|:---------------:|:------:|:------:|:-----:|:--------------------------------------------------------------------------------------------:|
|ProductionSetName|`string`|☑️      |       |The production set to remove results from                                                     |
|SearchTerm       |`string`|        |       |The search term to use for choosing which items to remove. If null, all items will be removed.|
|CasePath         |`string`|☑️      |       |The path of the case to search                                                                |
## NuixSearchAndTag

Searches a NUIX case with a particular search string and tags all files it finds

|Parameter |Type    |Required|Default|Summary                           |
|:--------:|:------:|:------:|:-----:|:--------------------------------:|
|Tag       |`string`|☑️      |       |The tag to assign to found results|
|SearchTerm|`string`|☑️      |       |The term to search for            |
|CasePath  |`string`|☑️      |       |The path of the case to search    |
## NuixCaseExists

Asserts that a particular case exists or doesn't exist.

|Parameter  |Type    |Required|Default|Summary                                                                                   |
|:---------:|:------:|:------:|:-----:|:----------------------------------------------------------------------------------------:|
|ShouldExist|`bool`  |☑️      |True   |If true, asserts that the case does exist. If false, asserts that the case does not exist.|
|CasePath   |`string`|☑️      |       |The case path to check                                                                    |
## NuixCount

Asserts that a particular number of items match a particular search term.

|Parameter    |Type    |Required|Default|Summary                                                                                   |
|:-----------:|:------:|:------:|:-----:|:----------------------------------------------------------------------------------------:|
|ExpectedCount|`int`   |☑️      |0      |If true, asserts that the case does exist. If false, asserts that the case does not exist.|
|CasePath     |`string`|☑️      |       |The case path to check                                                                    |
|SearchTerm   |`string`|☑️      |       |The search term to count                                                                  |
## Conditional

A process that runs a process depending on the success of an assertion.

|Parameter|Type     |Required|Default|Summary                                            |
|:-------:|:-------:|:------:|:-----:|:-------------------------------------------------:|
|If       |`Process`|☑️      |       |The process to use as the assertion                |
|Then     |`Process`|☑️      |       |If the 'If' process was successful then run this.  |
|Else     |`Process`|        |       |If the 'If' process was unsuccessful then run this.|
## DeleteFile

Deletes a file or a directory.

|Parameter|Type    |Required|Default|Summary                                     |
|:-------:|:------:|:------:|:-----:|:------------------------------------------:|
|Path     |`string`|☑️      |       |The path to the file or directory to delete.|
## ForEach

Performs a nested process once for each element in an enumeration

|Parameter  |Type         |Required|Default|Summary                                 |
|:---------:|:-----------:|:------:|:-----:|:--------------------------------------:|
|Enumeration|`Enumeration`|☑️      |       |The enumeration to iterate through.     |
|SubProcess |`Process`    |☑️      |       |The process to run once for each element|
## RunExternalProcess

Runs an external process

|Parameter          |Type                       |Required|Default|Summary                                                                        |
|:-----------------:|:-------------------------:|:------:|:-----:|:-----------------------------------------------------------------------------:|
|ProcessPath        |`string`                   |☑️      |       |The path to the process to run                                                 |
|Parameters         |`Dictionary<string,string>`|☑️      |       |Pairs of parameters to give to the process                                     |
|ExtraParameterName |`string`                   |        |       |The name of an additional parameter. This is intended for use with injection.  |
|ExtraParameterValue|`string`                   |        |       |The value of the additional parameter. This is intended for use with injection.|
## Sequence

Executes each step in sequence until a condition is not met or a process fails.

|Parameter|Type           |Required|Default|Summary                                                 |
|:-------:|:-------------:|:------:|:-----:|:------------------------------------------------------:|
|Steps    |`List<Process>`|☑️      |       |Steps that make up this process. To be executed in order|
## DeduplicateBy
Whether to deduplicate as a family or individual

|Name      |Summary                                                                                                                                                                                                                                                                           |
|:--------:|:--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:|
|Individual|Deduplication by individual treats each item as an individual and an attachment or embedded item has the same priority for deduplication as a loose file.                                                                                                                         |
|Family    |Items can be treated as a family where only the top-level item of a family is deduplicated and the descendants are classified as original or duplicate with their family as a group. The top-level item does not have to be in the set for its descendants to classified this way.|
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
## ProductionSetSortOrder
Selects the method of sorting items during production set sort ordering

|Name                      |Summary                                                                                   |
|:------------------------:|:----------------------------------------------------------------------------------------:|
|Position                  |Default sort order (fastest). Sorts as documented in ItemSorter.sortItemsByPosition(List).|
|TopLevelItemDate          |Sorts as documented in ItemSorter.sortItemsByTopLevelItemDate(List).                      |
|TopLevelItemDateDescending|Sorts as documented in ItemSorter.sortItemsByTopLevelItemDateDescending(List).            |
|DocumentId                |Sorts items based on their document IDs for the production set.                           |
