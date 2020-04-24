# Yaml
<a name="RunProcessFromYaml"></a>
## RunProcessFromYaml

Run process defined in a yaml file.

|Parameter|Type    |Required|Summary               |
|:-------:|:------:|:------:|:--------------------:|
|yamlPath |`string`|☑️      |Path to the yaml file.|

<a name="RunProcessFromYamlString"></a>
## RunProcessFromYamlString

Run process defined in a yaml string.

|Parameter |Type    |Required|Summary                       |
|:--------:|:------:|:------:|:----------------------------:|
|yamlString|`string`|☑️      |Yaml representing the process.|

# Scripts
<a name="GenerateScripts"></a>
## GenerateScripts

Generates ruby scripts for all RubyScriptProcesses in the AppDomain.

|Parameter |Type    |Required|Summary                                     |
|:--------:|:------:|:------:|:------------------------------------------:|
|folderPath|`string`|☑️      |Path to the folder to create the scripts in.|

<a name="Process"></a>
# General Processes
<a name="AssertError"></a>
## AssertError

**Unit**

Asserts that a particular process will produce an error.

|Parameter|Type               |Required|Summary                              |
|:-------:|:-----------------:|:------:|:-----------------------------------:|
|Process  |[Process](#Process)|☑️      |The process that is expected to fail.|

<a name="AssertFalse"></a>
## AssertFalse

**Unit**

Asserts that the Check will return false.

|Parameter|Type               |Required|Summary                                                                       |
|:-------:|:-----------------:|:------:|:----------------------------------------------------------------------------:|
|ResultOf |[Process](#Process)|        |The process whose result should be checked. Should have a return type of bool.|

<a name="AssertTrue"></a>
## AssertTrue

**Unit**

Asserts that the Check will return true.

|Parameter|Type               |Required|Summary                                                                       |
|:-------:|:-----------------:|:------:|:----------------------------------------------------------------------------:|
|ResultOf |[Process](#Process)|        |The process whose result should be checked. Should have a return type of bool.|

<a name="CheckNumber"></a>
## CheckNumber

**Boolean**

Checks that the count of the Check is within a particular range.

|Parameter|Type               |Required|Summary                                                                            |
|:-------:|:-----------------:|:------:|:---------------------------------------------------------------------------------:|
|Minimum  |`int`?             |        |Inclusive minimum of the expected range. Either this, Maximum, or both must be set.|
|Maximum  |`int`?             |        |Inclusive maximum of the expected range. Either this, Minimum, or both must be set.|
|Check    |[Process](#Process)|        |The process whose count should be checked. Should have a return type of int.       |

<a name="Conditional"></a>
## Conditional

**Returns the same type as the 'Then' and 'Else' processes. Returns void if there is no Else process.**

Runs the 'If' process. If it completed successfully then run the 'Then' process, otherwise run the 'Else' process.

|Parameter|Type               |Required|Summary                                                                                                                                             |
|:-------:|:-----------------:|:------:|:--------------------------------------------------------------------------------------------------------------------------------------------------:|
|If       |[Process](#Process)|☑️      |The process to use as the assertion. Must have the boolean result type.                                                                             |
|Then     |[Process](#Process)|☑️      |If the 'If' process was successful then run this. Must have the same result type as the 'Else' process, if there is one and the void type otherwise.|
|Else     |[Process](#Process)|        |If the 'If' process was unsuccessful then run this. Must have the same result type as the 'Then' process.                                           |

<a name="CreateDirectory"></a>
## CreateDirectory

**Unit**

Creates a new directory in the file system.

|Parameter|Type    |Required|Summary                             |
|:-------:|:------:|:------:|:----------------------------------:|
|Path     |`string`|☑️      |The path to the directory to create.|

<a name="DeleteItem"></a>
## DeleteItem

**Unit**

Deletes a file or a directory.

|Parameter|Type    |Required|Summary                                     |
|:-------:|:------:|:------:|:------------------------------------------:|
|Path     |`string`|☑️      |The path to the file or directory to delete.|

<a name="DoesFileContain"></a>
## DoesFileContain

**Boolean**

Checks whether a particular file contains a particular string.

|Parameter       |Type    |Required|Summary                           |
|:--------------:|:------:|:------:|:--------------------------------:|
|ExpectedContents|`string`|☑️      |The file must contain this string.|
|FilePath        |`string`|☑️      |The path to the file to check.    |

<a name="Loop"></a>
## Loop

**Unit**

Performs a nested process once for each element in an enumeration.

|Parameter|Type                       |Required|Summary                                  |
|:-------:|:-------------------------:|:------:|:---------------------------------------:|
|For      |[Enumeration](#Enumeration)|☑️      |The enumeration to iterate through.      |
|Do       |[Process](#Process)        |☑️      |The process to run once for each element.|

<a name="RunExternalProcess"></a>
## RunExternalProcess

**Unit**

Runs an external process.

|Parameter  |Type          |Required|Summary                          |Default                                         |
|:---------:|:------------:|:------:|:-------------------------------:|:----------------------------------------------:|
|ProcessPath|`string`      |☑️      |The path to the process to run.  |                                                |
|Arguments  |List<`string`>|        |Arguments to give to the process.|System.Collections.Generic.List`1[System.String]|

<a name="Sequence"></a>
## Sequence

**Unit**

Executes each step, one after the another. Will stop if a process fails.

|Parameter|Type                     |Required|Summary                                                                  |
|:-------:|:-----------------------:|:------:|:-----------------------------------------------------------------------:|
|Steps    |List<[Process](#Process)>|☑️      |Steps that make up this sequence. These should all have result type void.|

<a name="Unzip"></a>
## Unzip

**Unit**

Unzips a file.

|Parameter           |Type    |Required|Summary                                                          |Default|
|:------------------:|:------:|:------:|:---------------------------------------------------------------:|:-----:|
|ArchiveFilePath     |`string`|☑️      |The path to the archive to unzip.                                |       |
|DestinationDirectory|`string`|☑️      |The path to the directory in which to place the extracted files. |       |
|OverwriteFiles      |`bool`  |        |Should files be overwritten in the destination directory.        |False  |

<a name="WriteFile"></a>
## WriteFile

**Unit**

Writes the output of a process to a file. Will overwrite if necessary.

|Parameter|Type               |Required|Summary                                                                                 |
|:-------:|:-----------------:|:------:|:--------------------------------------------------------------------------------------:|
|Text     |[Process](#Process)|        |The process whose result is the text to be written. Should have a return type of string.|

<a name="Enumeration"></a>
# Enumerations
<a name="ColumnInjection"></a>
## ColumnInjection

Injects values from a CSV column into a property of a loop's process.

|Parameter|Type    |Required|Summary                                                                                                                                                                |Default                                 |Example   |
|:-------:|:------:|:------:|:---------------------------------------------------------------------------------------------------------------------------------------------------------------------:|:--------------------------------------:|:--------:|
|Column   |`string`|☑️      |The column in the CSV to get the values from.                                                                                                                          |                                        |SearchTerm|
|Property |`string`|☑️      |The property of the subProcess to inject.                                                                                                                              |                                        |SearchTerm|
|Regex    |`string`|        |The regex to use to extract the useful part of the element. The first match of the regex will be used.                                                                 |*The entire value will be injected.*    |\w+       |
|Template |`string`|        |The template to apply to the element before injection. The string '$1' in the template will be replaced with the element. The template will be applied after the Regex.|*The value will be injected on its own.*|$1.txt    |

<a name="CSV"></a>
## CSV

Enumerates through a CSV file.

|Parameter                |Type                                     |Required|Summary                                                                                                                         |Default|
|:-----------------------:|:---------------------------------------:|:------:|:------------------------------------------------------------------------------------------------------------------------------:|:-----:|
|ColumnInjections         |List<[ColumnInjection](#ColumnInjection)>|☑️      |List of columns and properties to inject.                                                                                       |       |
|CommentToken             |`string`                                 |        |A string that, when placed at the beginning of a line, indicates that the line is a comment and should be ignored by the parser.|       |
|CSVFilePath              |`string`                                 |        |The path to the CSV file. Either this, CSVText, or CSVProcess must be set (but not more than one).                              |       |
|CSVProcess               |[Process](#Process)                      |        |A process which produces a string in CSV format. Either this, CSVFilePath, or CSVText must be set (but not more than one).      |       |
|CSVText                  |`string`                                 |        |Raw CSV. Either this, CSVFilePath, or CSVProcess must be set (but not more than one).                                           |       |
|Delimiter                |`string`                                 |        |The delimiter used in the CSV file.                                                                                             |,      |
|Distinct                 |`bool`                                   |        |Whether to only enumerate unique values from the CSV. Uniqueness is determined only from the columns which are being injected.  |False  |
|HasFieldsEnclosedInQuotes|`bool`                                   |        |Determines whether fields are enclosed in quotation marks.                                                                      |False  |

<a name="Directory"></a>
## Directory

Enumerates through files in a directory.

|Parameter|Type                         |Required|Summary                    |
|:-------:|:---------------------------:|:------:|:-------------------------:|
|Path     |`string`                     |☑️      |The path to the directory. |
|Injection|List<[Injection](#Injection)>|☑️      |Property injections to use.|

<a name="Injection"></a>
## Injection

Injects a value from the enumerator into a property of a loop's process.

|Parameter|Type    |Required|Summary                                                                                                                                                                |Default                                 |Example   |
|:-------:|:------:|:------:|:---------------------------------------------------------------------------------------------------------------------------------------------------------------------:|:--------------------------------------:|:--------:|
|Property |`string`|☑️      |The property of the subProcess to inject.                                                                                                                              |                                        |SearchTerm|
|Regex    |`string`|        |The regex to use to extract the useful part of the element. The first match of the regex will be used.                                                                 |*The entire value will be injected.*    |\w+       |
|Template |`string`|        |The template to apply to the element before injection. The string '$1' in the template will be replaced with the element. The template will be applied after the Regex.|*The value will be injected on its own.*|$1.txt    |

<a name="List"></a>
## List

Enumerates through elements of a list.

|Parameter|Type                         |Required|Summary                      |
|:-------:|:---------------------------:|:------:|:---------------------------:|
|Members  |List<`string`>               |☑️      |The elements to iterate over.|
|Inject   |List<[Injection](#Injection)>|☑️      |Property injections to use.  |

# Nuix Processes
<a name="NuixAddConcordance"></a>
## NuixAddConcordance

**Unit**

*Requires Nuix Version 7.6*

*Requires Nuix Feature 'CASE_CREATION'*

*Requires Nuix Feature 'METADATA_IMPORT'*

Adds data from a Concordance file to a NUIX case.

|Parameter             |Type    |Required|Summary                                           |Example                   |
|:--------------------:|:------:|:------:|:------------------------------------------------:|:------------------------:|
|ConcordanceProfileName|`string`|☑️      |The name of the concordance profile to use.       |MyProfile                 |
|ConcordanceDateFormat |`string`|☑️      |The concordance date format to use.               |yyyy-MM-dd'T'HH:mm:ss.SSSZ|
|FilePath              |`string`|☑️      |The path of the concordance file to import.       |C:/MyConcordance.dat      |
|Custodian             |`string`|☑️      |The name of the custodian to assign the folder to.|                          |
|Description           |`string`|        |A description to add to the folder.               |                          |
|FolderName            |`string`|☑️      |The name of the folder to create.                 |                          |
|CasePath              |`string`|☑️      |The path to the case to import into.              |C:/Cases/MyCase           |

<a name="NuixAddItem"></a>
## NuixAddItem

**Unit**

*Requires Nuix Version 5.0*

*Requires Nuix Feature 'CASE_CREATION'*

Adds a file or directory to a Nuix Case.

|Parameter            |Type    |Required|Summary                                                       |Default                                       |Example                            |Requirements|
|:-------------------:|:------:|:------:|:------------------------------------------------------------:|:--------------------------------------------:|:---------------------------------:|:----------:|
|Path                 |`string`|☑️      |The path of the file or directory to add to the case.         |                                              |C:/Data/File.txt                   |            |
|Custodian            |`string`|☑️      |The custodian to assign to the new folder.                    |                                              |                                   |            |
|Description          |`string`|        |The description of the new folder.                            |                                              |                                   |            |
|FolderName           |`string`|☑️      |The name of the folder to create.                             |                                              |                                   |            |
|CasePath             |`string`|☑️      |The path to the case.                                         |                                              |C:/Cases/MyCase                    |            |
|PasswordFilePath     |`string`|☑️      |The path of a file containing passwords to use for decryption.|                                              |C:/Data/Passwords.txt              |Nuix 7.6    |
|ProcessingProfileName|`string`|        |The name of the Processing profile to use.                    |*The default processing profile will be used.*|MyProcessingProfile                |Nuix 7.6    |
|ProcessingProfilePath|`string`|        |The path to the Processing profile to use                     |*The default processing profile will be used.*|C:/Profiles/MyProcessingProfile.xml|Nuix 7.6    |

<a name="NuixAddToItemSet"></a>
## NuixAddToItemSet

**Unit**

*Requires Nuix Version 5.0*

*Requires Nuix Feature 'ANALYSIS'*

Searches a case with a particular search string and adds all items it finds to a particular item set. Will create a new item set if one doesn't already exist.

|Parameter           |Type                                         |Required|Summary                                                                                                                                                                                      |Default   |Example                 |
|:------------------:|:-------------------------------------------:|:------:|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:|:--------:|:----------------------:|
|ItemSetName         |`string`                                     |☑️      |The item set to add results to. Will be created if it doesn't already exist.                                                                                                                 |          |                        |
|SearchTerm          |`string`                                     |☑️      |The term to search for.                                                                                                                                                                      |          |                        |
|CasePath            |`string`                                     |☑️      |The path of the case to search.                                                                                                                                                              |          |C:/Cases/MyCase         |
|ItemSetDeduplication|[ItemSetDeduplication](#ItemSetDeduplication)|        |The means of deduplicating items by key and prioritizing originals in a tie-break.                                                                                                           |Default   |                        |
|ItemSetDescription  |`string`                                     |        |The description of the item set.                                                                                                                                                             |          |                        |
|Order               |`string`                                     |        |How to order the items to be added to the item set.                                                                                                                                          |          |name ASC, item-date DESC|
|DeduplicateBy       |[DeduplicateBy](#DeduplicateBy)              |        |Whether to deduplicate as a family or individual.                                                                                                                                            |Individual|                        |
|Limit               |`int`?                                       |        |The maximum number of items to add to the item set.                                                                                                                                          |          |                        |
|CustodianRanking    |List<`string`>                               |        |A list of custodian names ordered from highest ranked to lowest ranked. If this parameter is present and the deduplication parameter has not been specified, MD5 Ranked Custodian is assumed.|          |                        |

<a name="NuixAddToProductionSet"></a>
## NuixAddToProductionSet

**Unit**

*Requires Nuix Version 7.2*

*Requires Nuix Feature 'PRODUCTION_SET'*

Searches a case with a particular search string and adds all items it finds to a production set. Will create a new production set if one with the given name does not already exist.

|Parameter            |Type    |Required|Summary                                                                                         |Default                                       |Example                            |Requirements|
|:-------------------:|:------:|:------:|:----------------------------------------------------------------------------------------------:|:--------------------------------------------:|:---------------------------------:|:----------:|
|ProductionSetName    |`string`|☑️      |The production set to add results to. Will be created if it doesn't already exist               |                                              |                                   |            |
|SearchTerm           |`string`|☑️      |The term to search for                                                                          |                                              |                                   |            |
|CasePath             |`string`|☑️      |The path of the case to search                                                                  |                                              |C:/Cases/MyCase                    |            |
|Description          |`string`|        |Description of the production set.                                                              |                                              |                                   |            |
|Order                |`string`|        |How to order the items to be added to the production set.                                       |                                              |name ASC, item-date DESC           |            |
|Limit                |`int`?  |        |The maximum number of items to add to the production set.                                       |                                              |                                   |            |
|ProductionProfileName|`string`|        |The name of the Production profile to use. Either this or the ProductionProfilePath must be set |*The default processing profile will be used.*|MyProcessingProfile                |Nuix 7.2    |
|ProductionProfilePath|`string`|        |The path to the Production profile to use. Either this or the ProductionProfileName must be set.|*The default processing profile will be used.*|C:/Profiles/MyProcessingProfile.xml|Nuix 7.6    |

<a name="NuixAnnotateDocumentIdList"></a>
## NuixAnnotateDocumentIdList

**Unit**

*Requires Nuix Version 7.4*

*Requires Nuix Feature 'PRODUCTION_SET'*

Annotates a document ID list to add production set names to it.

|Parameter        |Type    |Required|Summary                                         |Example        |
|:---------------:|:------:|:------:|:----------------------------------------------:|:-------------:|
|ProductionSetName|`string`|☑️      |The production set to get names from.           |               |
|CasePath         |`string`|☑️      |The path to the case.                           |C:/Cases/MyCase|
|DataPath         |`string`|☑️      |Specifies the file path of the document ID list.|               |

<a name="NuixAssertPrintPreviewState"></a>
## NuixAssertPrintPreviewState

**Unit**

*Requires Nuix Version 5.2*

*Requires Nuix Feature 'ANALYSIS'*

*Requires Nuix Feature 'PRODUCTION_SET'*

Checks the print preview state of the production set.

|Parameter        |Type                                   |Required|Summary                                                |Default|Example        |
|:---------------:|:-------------------------------------:|:------:|:-----------------------------------------------------:|:-----:|:-------------:|
|ExpectedState    |[PrintPreviewState](#PrintPreviewState)|        |The expected print preview state of the production set;|All    |               |
|ProductionSetName|`string`                               |☑️      |The production set to reorder.                         |       |               |
|CasePath         |`string`                               |☑️      |The path to the case.                                  |       |C:/Cases/MyCase|

<a name="NuixAssignCustodian"></a>
## NuixAssignCustodian

**Unit**

*Requires Nuix Version 5.0*

*Requires Nuix Feature 'ANALYSIS'*

Searches a NUIX case with a particular search string and assigns all files it finds to a particular custodian.

|Parameter |Type    |Required|Summary                                  |Example        |
|:--------:|:------:|:------:|:---------------------------------------:|:-------------:|
|Custodian |`string`|☑️      |The custodian to assign to found results.|               |
|SearchTerm|`string`|☑️      |The term to search for.                  |*.txt          |
|CasePath  |`string`|☑️      |The path to the case.                    |C:/Cases/MyCase|

<a name="NuixCountItems"></a>
## NuixCountItems

**Unit**

*Requires Nuix Version 5.0*

Returns the number of items matching a particular search term

|Parameter |Type    |Required|Summary                  |Example        |
|:--------:|:------:|:------:|:-----------------------:|:-------------:|
|CasePath  |`string`|☑️      |The path to the case.    |C:/Cases/MyCase|
|SearchTerm|`string`|☑️      |The search term to count.|*.txt          |

<a name="NuixCreateCase"></a>
## NuixCreateCase

**Unit**

*Requires Nuix Version 5.0*

*Requires Nuix Feature 'CASE_CREATION'*

Creates a new case.

|Parameter   |Type    |Required|Summary                                      |Example        |
|:----------:|:------:|:------:|:-------------------------------------------:|:-------------:|
|CaseName    |`string`|☑️      |The name of the case to create.              |               |
|CasePath    |`string`|☑️      |The path to the folder to create the case in.|C:/Cases/MyCase|
|Investigator|`string`|☑️      |Name of the investigator.                    |               |
|Description |`string`|        |Description of the case.                     |               |

<a name="NuixCreateIrregularItemsReport"></a>
## NuixCreateIrregularItemsReport

**Unit**

*Requires Nuix Version 5.0*

Creates a list of all irregular items in a case. The report is in CSV format. The headers are 'Reason', 'Path' and 'Guid' Reasons include 'NonSearchablePDF','BadExtension','Unrecognised','Unsupported','TextNotIndexed','ImagesNotProcessed','Poisoned','Record','UnrecognisedDeleted','NeedManualExamination', and 'CodeTextFiles' Use this inside a WriteFile process to write it to a file.

|Parameter|Type    |Required|Summary              |Example        |
|:-------:|:------:|:------:|:-------------------:|:-------------:|
|CasePath |`string`|☑️      |The path to the case.|C:/Cases/MyCase|

<a name="NuixCreateNRTReport"></a>
## NuixCreateNRTReport

**Unit**

*Requires Nuix Version 7.4*

*Requires Nuix Feature 'ANALYSIS'*

Creates a report using an NRT file.

|Parameter        |Type    |Required|Summary                                                        |Example                                                                 |
|:---------------:|:------:|:------:|:-------------------------------------------------------------:|:----------------------------------------------------------------------:|
|CasePath         |`string`|☑️      |The path to the case.                                          |C:/Cases/MyCase                                                         |
|NRTPath          |`string`|☑️      |The NRT file path.                                             |                                                                        |
|OutputFormat     |`string`|☑️      |The format of the report file that will be created.            |PDF                                                                     |
|LocalResourcesURL|`string`|☑️      |The path to the local resources folder. To load the logo's etc.|C:\Program Files\Nuix\Nuix 8.4\user-data\Reports\Case Summary\Resources\|
|OutputPath       |`string`|☑️      |The path to output the file at.                                |C:/Temp/report.pdf                                                      |

<a name="NuixCreateReport"></a>
## NuixCreateReport

**Unit**

*Requires Nuix Version 6.2*

*Requires Nuix Feature 'ANALYSIS'*

Creates a report for a Nuix case. The report is in csv format. The headers are 'Custodian', 'Type', 'Value', and 'Count'. The different types are: 'Kind', 'Type', 'Tag', and 'Address'. Use this inside a WriteFile process to write it to a file.

|Parameter|Type    |Required|Summary              |Example        |
|:-------:|:------:|:------:|:-------------------:|:-------------:|
|CasePath |`string`|☑️      |The path to the case.|C:/Cases/MyCase|

<a name="NuixCreateTermList"></a>
## NuixCreateTermList

**Unit**

*Requires Nuix Version 5.0*

Creates a list of all terms appearing in the case and their frequencies. The report is in CSV format. The headers are 'Term' and 'Count' Use this inside a WriteFile process to write it to a file.

|Parameter|Type    |Required|Summary              |Example        |
|:-------:|:------:|:------:|:-------------------:|:-------------:|
|CasePath |`string`|☑️      |The path to the case.|C:/Cases/MyCase|

<a name="NuixDoesCaseExists"></a>
## NuixDoesCaseExists

**Unit**

*Requires Nuix Version 5.0*

Returns whether or not a case exists.

|Parameter|Type    |Required|Summary              |Example        |
|:-------:|:------:|:------:|:-------------------:|:-------------:|
|CasePath |`string`|☑️      |The path to the case.|C:/Cases/MyCase|

<a name="NuixExportConcordance"></a>
## NuixExportConcordance

**Unit**

*Requires Nuix Version 7.2*

*Requires Nuix Feature 'EXPORT_ITEMS'*

*Requires Nuix Feature 'PRODUCTION_SET'*

Exports Concordance for a particular production set.

|Parameter        |Type    |Required|Summary                                  |Example        |
|:---------------:|:------:|:------:|:---------------------------------------:|:-------------:|
|ProductionSetName|`string`|☑️      |The name of the production set to export.|               |
|ExportPath       |`string`|☑️      |Where to export the Concordance to.      |               |
|CasePath         |`string`|☑️      |The path to the case.                    |C:/Cases/MyCase|

<a name="NuixExtractEntities"></a>
## NuixExtractEntities

**Unit**

*Requires Nuix Version 5.0*

Extract Entities from a Nuix Case.

|Parameter   |Type    |Required|Summary                                           |Example        |
|:----------:|:------:|:------:|:------------------------------------------------:|:-------------:|
|OutputFolder|`string`|☑️      |The path to the folder to put the output files in.|C:/Output      |
|CasePath    |`string`|☑️      |The path to the case.                             |C:/Cases/MyCase|

<a name="NuixGeneratePrintPreviews"></a>
## NuixGeneratePrintPreviews

**Unit**

*Requires Nuix Version 5.2*

*Requires Nuix Feature 'PRODUCTION_SET'*

Generates print previews for items in a production set.

|Parameter        |Type    |Required|Summary                                           |Example        |
|:---------------:|:------:|:------:|:------------------------------------------------:|:-------------:|
|ProductionSetName|`string`|☑️      |The production set to generate print previews for.|               |
|CasePath         |`string`|☑️      |The path to the case.                             |C:/Cases/MyCase|

<a name="NuixGetItemProperties"></a>
## NuixGetItemProperties

**Unit**

*Requires Nuix Version 6.2*

A process that the searches a case for items and outputs the values of item properties. The report is in CSV format. The headers are 'Key', 'Value', 'Path' and 'Guid' Use this inside a WriteFile process to write it to a file.

|Parameter    |Type    |Required|Summary                                                                                                                                                     |Example        |
|:-----------:|:------:|:------:|:----------------------------------------------------------------------------------------------------------------------------------------------------------:|:-------------:|
|CasePath     |`string`|☑️      |The path to the case.                                                                                                                                       |C:/Cases/MyCase|
|SearchTerm   |`string`|☑️      |The term to search for.                                                                                                                                     |*.txt          |
|PropertyRegex|`string`|☑️      |The regex to search the property for.                                                                                                                       |Date           |
|ValueRegex   |`string`|        |An optional regex to check the value. If this is set, only values which match this regex will be returned, and only the contents of the first capture group.|(199\d)        |

<a name="NuixImportDocumentIds"></a>
## NuixImportDocumentIds

**Unit**

*Requires Nuix Version 7.4*

*Requires Nuix Feature 'PRODUCTION_SET'*

Imports the given document IDs into this production set. Only works if this production set has imported numbering.

|Parameter                    |Type    |Required|Summary                                                                                |Default|Example        |
|:---------------------------:|:------:|:------:|:-------------------------------------------------------------------------------------:|:-----:|:-------------:|
|ProductionSetName            |`string`|☑️      |The production set to add results to.                                                  |       |               |
|CasePath                     |`string`|☑️      |The path to the case.                                                                  |       |C:/Cases/MyCase|
|AreSourceProductionSetsInData|`bool`  |        |Specifies that the source production set name(s) are contained in the document ID list.|False  |               |
|DataPath                     |`string`|☑️      |Specifies the file path of the document ID list.                                       |       |               |

<a name="NuixMigrateCase"></a>
## NuixMigrateCase

**Unit**

*Requires Nuix Version 5.0*

Migrates a case to the latest version if necessary.

|Parameter|Type    |Required|Summary              |Example        |
|:-------:|:------:|:------:|:-------------------:|:-------------:|
|CasePath |`string`|☑️      |The path to the case.|C:/Cases/MyCase|

<a name="NuixPerformOCR"></a>
## NuixPerformOCR

**Unit**

*Requires Nuix Version 7.6*

*Requires Nuix Feature 'OCR_PROCESSING'*

Performs optical character recognition on files in a NUIX case.

|Parameter     |Type    |Required|Summary                                                                                   |Default                                                                                                                                         |Example                  |Requirements|
|:------------:|:------:|:------:|:----------------------------------------------------------------------------------------:|:----------------------------------------------------------------------------------------------------------------------------------------------:|:-----------------------:|:----------:|
|CasePath      |`string`|☑️      |The path to the case.                                                                     |                                                                                                                                                |C:/Cases/MyCase          |            |
|SearchTerm    |`string`|        |The term to use for searching for files to OCR.                                           |NOT flag:encrypted AND ((mime-type:application/pdf AND NOT content:*) OR (mime-type:image/* AND ( flag:text_not_indexed OR content:( NOT * ) )))|                         |            |
|OCRProfileName|`string`|        |The name of the OCR profile to use. This cannot be set at the same time as OCRProfilePath.|*The default profile will be used.*                                                                                                             |MyOcrProfile             |            |
|OCRProfilePath|`string`|        |Path to the OCR profile to use. This cannot be set at the same times as OCRProfileName.   |*The default profile will be used.*                                                                                                             |C:\Profiles\MyProfile.xml|Nuix 7.6    |

<a name="NuixRemoveFromProductionSet"></a>
## NuixRemoveFromProductionSet

**Unit**

*Requires Nuix Version 5.0*

*Requires Nuix Feature 'PRODUCTION_SET'*

Removes particular items from a Nuix production set.

|Parameter        |Type    |Required|Summary                                                   |Default                     |Example        |
|:---------------:|:------:|:------:|:--------------------------------------------------------:|:--------------------------:|:-------------:|
|ProductionSetName|`string`|☑️      |The production set to remove results from.                |                            |               |
|SearchTerm       |`string`|        |The search term to use for choosing which items to remove.|*All items will be removed.*|Tag:sushi      |
|CasePath         |`string`|☑️      |The path to the case.                                     |                            |C:/Cases/MyCase|

<a name="NuixReorderProductionSet"></a>
## NuixReorderProductionSet

**Unit**

*Requires Nuix Version 5.2*

*Requires Nuix Feature 'PRODUCTION_SET'*

Reorders and renumbers the items in a production set.

|Parameter        |Type                                             |Required|Summary                                            |Default |Example        |
|:---------------:|:-----------------------------------------------:|:------:|:-------------------------------------------------:|:------:|:-------------:|
|ProductionSetName|`string`                                         |☑️      |The production set to reorder.                     |        |               |
|CasePath         |`string`                                         |☑️      |The path to the case.                              |        |C:/Cases/MyCase|
|SortOrder        |[ProductionSetSortOrder](#ProductionSetSortOrder)|        |The method of sorting items during the renumbering.|Position|               |

<a name="NuixSearchAndTag"></a>
## NuixSearchAndTag

**Unit**

*Requires Nuix Version 5.0*

*Requires Nuix Feature 'ANALYSIS'*

Searches a NUIX case with a particular search string and tags all files it finds.

|Parameter |Type    |Required|Summary                            |Example        |
|:--------:|:------:|:------:|:---------------------------------:|:-------------:|
|Tag       |`string`|☑️      |The tag to assign to found results.|               |
|SearchTerm|`string`|☑️      |The term to search for.            |*.txt          |
|CasePath  |`string`|☑️      |The path to the case.              |C:/Cases/MyCase|

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

