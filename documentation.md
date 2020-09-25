# Nuix
<a name="NuixAddConcordance"></a>
## NuixAddConcordance

**Unit**

*Requires Nuix Version 7.6*

*Requires NuixCASE_CREATION*

*Requires NuixMETADATA_IMPORT*

Adds data from a Concordance file to a NUIX case.

|Parameter             |Type    |Required|Summary                                           |Example                   |
|:--------------------:|:------:|:------:|:------------------------------------------------:|:------------------------:|
|CasePath              |`string`|☑️      |The path to the case to import into.              |C:/Cases/MyCase           |
|ConcordanceDateFormat |`string`|☑️      |The concordance date format to use.               |yyyy-MM-dd'T'HH:mm:ss.SSSZ|
|ConcordanceProfileName|`string`|☑️      |The name of the concordance profile to use.       |MyProfile                 |
|Custodian             |`string`|☑️      |The name of the custodian to assign the folder to.|                          |
|Description           |`string`|        |A description to add to the folder.               |                          |
|FilePath              |`string`|☑️      |The path of the concordance file to import.       |C:/MyConcordance.dat      |
|FolderName            |`string`|☑️      |The name of the folder to create.                 |                          |

<a name="NuixAddItem"></a>
## NuixAddItem

**Unit**

*Requires Nuix Version 5.0*

*Requires NuixCASE_CREATION*

Adds a file or directory to a Nuix Case.

|Parameter            |Type    |Required|Summary                                                       |Default Value                               |Example                            |Requirements|
|:-------------------:|:------:|:------:|:------------------------------------------------------------:|:------------------------------------------:|:---------------------------------:|:----------:|
|CasePath             |`string`|☑️      |The path to the case.                                         |                                            |C:/Cases/MyCase                    |            |
|Custodian            |`string`|☑️      |The custodian to assign to the new folder.                    |                                            |                                   |            |
|Description          |`string`|        |The description of the new folder.                            |                                            |                                   |            |
|FolderName           |`string`|☑️      |The name of the folder to create.                             |                                            |                                   |            |
|PasswordFilePath     |`string`|        |The path of a file containing passwords to use for decryption.|                                            |C:/Data/Passwords.txt              |Nuix 7.6    |
|Path                 |`string`|☑️      |The path of the file or directory to add to the case.         |                                            |C:/Data/File.txt                   |            |
|ProcessingProfileName|`string`|        |The name of the Processing profile to use.                    |The default processing profile will be used.|MyProcessingProfile                |Nuix 7.6    |
|ProcessingProfilePath|`string`|        |The path to the Processing profile to use                     |The default processing profile will be used.|C:/Profiles/MyProcessingProfile.xml|Nuix 7.6    |

<a name="NuixAddToItemSet"></a>
## NuixAddToItemSet

**Unit**

*Requires Nuix Version 5.0*

*Requires NuixANALYSIS*

Searches a case with a particular search string and adds all items it finds to a particular item set. Will create a new item set if one doesn't already exist.

|Parameter           |Type                                         |Required|Summary                                                                                                                                                                                      |Example                 |
|:------------------:|:-------------------------------------------:|:------:|:-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------:|:----------------------:|
|CasePath            |`string`                                     |☑️      |The path of the case to search.                                                                                                                                                              |C:/Cases/MyCase         |
|CustodianRanking    |List<`string`>                               |        |A list of custodian names ordered from highest ranked to lowest ranked. If this parameter is present and the deduplication parameter has not been specified, MD5 Ranked Custodian is assumed.|                        |
|DeduplicateBy       |[DeduplicateBy](#DeduplicateBy)              |        |Whether to deduplicate as a family or individual.                                                                                                                                            |                        |
|ItemSetDeduplication|[ItemSetDeduplication](#ItemSetDeduplication)|        |The means of deduplicating items by key and prioritizing originals in a tie-break.                                                                                                           |                        |
|ItemSetDescription  |`string`                                     |        |The description of the item set.                                                                                                                                                             |                        |
|ItemSetName         |`string`                                     |☑️      |The item set to add results to. Will be created if it doesn't already exist.                                                                                                                 |                        |
|Limit               |`int`                                        |        |The maximum number of items to add to the item set.                                                                                                                                          |                        |
|Order               |`string`                                     |        |How to order the items to be added to the item set.                                                                                                                                          |name ASC, item-date DESC|
|SearchTerm          |`string`                                     |☑️      |The term to search for.                                                                                                                                                                      |                        |

<a name="NuixAddToProductionSet"></a>
## NuixAddToProductionSet

**Unit**

*Requires Nuix Version 7.2*

*Requires NuixPRODUCTION_SET*

Searches a case with a particular search string and adds all items it finds to a production set. Will create a new production set if one with the given name does not already exist.

|Parameter            |Type    |Required|Summary                                                                                         |Default Value                               |Example                            |Requirements|
|:-------------------:|:------:|:------:|:----------------------------------------------------------------------------------------------:|:------------------------------------------:|:---------------------------------:|:----------:|
|CasePath             |`string`|☑️      |The path of the case to search                                                                  |                                            |C:/Cases/MyCase                    |            |
|Description          |`string`|        |Description of the production set.                                                              |                                            |                                   |            |
|Limit                |`int`   |        |The maximum number of items to add to the production set.                                       |                                            |                                   |            |
|Order                |`string`|        |How to order the items to be added to the production set.                                       |                                            |name ASC, item-date DESC           |            |
|ProductionProfileName|`string`|        |The name of the Production profile to use. Either this or the ProductionProfilePath must be set |The default processing profile will be used.|MyProcessingProfile                |Nuix 7.2    |
|ProductionProfilePath|`string`|        |The path to the Production profile to use. Either this or the ProductionProfileName must be set.|The default processing profile will be used.|C:/Profiles/MyProcessingProfile.xml|Nuix 7.6    |
|ProductionSetName    |`string`|☑️      |The production set to add results to. Will be created if it doesn't already exist               |                                            |                                   |            |
|SearchTerm           |`string`|☑️      |The term to search for                                                                          |                                            |                                   |            |

<a name="NuixAnnotateDocumentIdList"></a>
## NuixAnnotateDocumentIdList

**Unit**

*Requires Nuix Version 7.4*

*Requires NuixPRODUCTION_SET*

Annotates a document ID list to add production set names to it.

|Parameter        |Type    |Required|Summary                                         |Example        |
|:---------------:|:------:|:------:|:----------------------------------------------:|:-------------:|
|CasePath         |`string`|☑️      |The path to the case.                           |C:/Cases/MyCase|
|DataPath         |`string`|☑️      |Specifies the file path of the document ID list.|               |
|ProductionSetName|`string`|☑️      |The production set to get names from.           |               |

<a name="NuixAssertPrintPreviewState"></a>
## NuixAssertPrintPreviewState

**Unit**

*Requires Nuix Version 5.2*

*Requires NuixPRODUCTION_SET*

*Requires NuixANALYSIS*

Checks the print preview state of the production set.

|Parameter        |Type                                   |Required|Summary                                                |Default Value|Example        |
|:---------------:|:-------------------------------------:|:------:|:-----------------------------------------------------:|:-----------:|:-------------:|
|CasePath         |`string`                               |☑️      |The path to the case.                                  |             |C:/Cases/MyCase|
|ExpectedState    |[PrintPreviewState](#PrintPreviewState)|        |The expected print preview state of the production set;|All          |               |
|ProductionSetName|`string`                               |☑️      |The production set to reorder.                         |             |               |

<a name="NuixAssignCustodian"></a>
## NuixAssignCustodian

**Unit**

*Requires Nuix Version 5.0*

*Requires NuixANALYSIS*

Searches a NUIX case with a particular search string and assigns all files it finds to a particular custodian.

|Parameter |Type    |Required|Summary                                  |Example        |
|:--------:|:------:|:------:|:---------------------------------------:|:-------------:|
|CasePath  |`string`|☑️      |The path to the case.                    |C:/Cases/MyCase|
|Custodian |`string`|☑️      |The custodian to assign to found results.|               |
|SearchTerm|`string`|☑️      |The term to search for.                  |\*.txt         |

<a name="NuixCountItems"></a>
## NuixCountItems

**Int32**

*Requires Nuix Version 5.0*

Returns the number of items matching a particular search term

|Parameter |Type    |Required|Summary                  |Example        |
|:--------:|:------:|:------:|:-----------------------:|:-------------:|
|CasePath  |`string`|☑️      |The path to the case.    |C:/Cases/MyCase|
|SearchTerm|`string`|☑️      |The search term to count.|\*.txt         |

<a name="NuixCreateCase"></a>
## NuixCreateCase

**Unit**

*Requires Nuix Version 5.0*

*Requires NuixCASE_CREATION*

Creates a new case.

|Parameter   |Type    |Required|Summary                                      |Example        |
|:----------:|:------:|:------:|:-------------------------------------------:|:-------------:|
|CaseName    |`string`|☑️      |The name of the case to create.              |               |
|CasePath    |`string`|☑️      |The path to the folder to create the case in.|C:/Cases/MyCase|
|Description |`string`|        |Description of the case.                     |               |
|Investigator|`string`|☑️      |Name of the investigator.                    |               |

<a name="NuixCreateIrregularItemsReport"></a>
## NuixCreateIrregularItemsReport

**String**

*Requires Nuix Version 5.0*

Creates a list of all irregular items in a case. The report is in CSV format. The headers are 'Reason', 'Path' and 'Guid' Reasons include 'NonSearchablePDF','BadExtension','Unrecognised','Unsupported','TextNotIndexed','ImagesNotProcessed','Poisoned','Record','UnrecognisedDeleted','NeedManualExamination', and 'CodeTextFiles' Use this inside a WriteFile process to write it to a file.

|Parameter|Type    |Required|Summary              |Example        |
|:-------:|:------:|:------:|:-------------------:|:-------------:|
|CasePath |`string`|☑️      |The path to the case.|C:/Cases/MyCase|

<a name="NuixCreateNRTReport"></a>
## NuixCreateNRTReport

**Unit**

*Requires Nuix Version 7.4*

*Requires NuixANALYSIS*

Creates a report using an NRT file.

|Parameter        |Type    |Required|Summary                                                        |Example                                                                         |
|:---------------:|:------:|:------:|:-------------------------------------------------------------:|:------------------------------------------------------------------------------:|
|CasePath         |`string`|☑️      |The path to the case.                                          |C:/Cases/MyCase                                                                 |
|LocalResourcesURL|`string`|☑️      |The path to the local resources folder. To load the logo's etc.|C:\\Program Files\\Nuix\\Nuix 8.4\\user-data\\Reports\\Case Summary\\Resources\\|
|NRTPath          |`string`|☑️      |The NRT file path.                                             |                                                                                |
|OutputFormat     |`string`|☑️      |The format of the report file that will be created.            |PDF                                                                             |
|OutputPath       |`string`|☑️      |The path to output the file at.                                |C:/Temp/report.pdf                                                              |

<a name="NuixCreateReport"></a>
## NuixCreateReport

**String**

*Requires Nuix Version 6.2*

*Requires NuixANALYSIS*

Creates a report for a Nuix case. The report is in csv format. The headers are 'Custodian', 'Type', 'Value', and 'Count'. The different types are: 'Kind', 'Type', 'Tag', and 'Address'. Use this inside a WriteFile process to write it to a file.

|Parameter|Type    |Required|Summary              |Example        |
|:-------:|:------:|:------:|:-------------------:|:-------------:|
|CasePath |`string`|☑️      |The path to the case.|C:/Cases/MyCase|

<a name="NuixCreateTermList"></a>
## NuixCreateTermList

**String**

*Requires Nuix Version 5.0*

Creates a list of all terms appearing in the case and their frequencies. The report is in CSV format. The headers are 'Term' and 'Count' Use this inside a WriteFile process to write it to a file.

|Parameter|Type    |Required|Summary              |Example        |
|:-------:|:------:|:------:|:-------------------:|:-------------:|
|CasePath |`string`|☑️      |The path to the case.|C:/Cases/MyCase|

<a name="NuixDoesCaseExists"></a>
## NuixDoesCaseExists

**Boolean**

*Requires Nuix Version 5.0*

Returns whether or not a case exists.

|Parameter|Type    |Required|Summary              |Example        |
|:-------:|:------:|:------:|:-------------------:|:-------------:|
|CasePath |`string`|☑️      |The path to the case.|C:/Cases/MyCase|

<a name="NuixExportConcordance"></a>
## NuixExportConcordance

**Unit**

*Requires Nuix Version 7.2*

*Requires NuixPRODUCTION_SET*

*Requires NuixEXPORT_ITEMS*

Exports Concordance for a particular production set.

|Parameter        |Type    |Required|Summary                                  |Example        |
|:---------------:|:------:|:------:|:---------------------------------------:|:-------------:|
|CasePath         |`string`|☑️      |The path to the case.                    |C:/Cases/MyCase|
|ExportPath       |`string`|☑️      |Where to export the Concordance to.      |               |
|ProductionSetName|`string`|☑️      |The name of the production set to export.|               |

<a name="NuixExtractEntities"></a>
## NuixExtractEntities

**Unit**

*Requires Nuix Version 5.0*

Extract Entities from a Nuix Case.

|Parameter   |Type    |Required|Summary                                           |Example        |
|:----------:|:------:|:------:|:------------------------------------------------:|:-------------:|
|CasePath    |`string`|☑️      |The path to the case.                             |C:/Cases/MyCase|
|OutputFolder|`string`|☑️      |The path to the folder to put the output files in.|C:/Output      |

<a name="NuixGeneratePrintPreviews"></a>
## NuixGeneratePrintPreviews

**Unit**

*Requires Nuix Version 5.2*

*Requires NuixPRODUCTION_SET*

Generates print previews for items in a production set.

|Parameter        |Type    |Required|Summary                                           |Example        |
|:---------------:|:------:|:------:|:------------------------------------------------:|:-------------:|
|CasePath         |`string`|☑️      |The path to the case.                             |C:/Cases/MyCase|
|ProductionSetName|`string`|☑️      |The production set to generate print previews for.|               |

<a name="NuixGetItemProperties"></a>
## NuixGetItemProperties

**String**

*Requires Nuix Version 6.2*

A process that the searches a case for items and outputs the values of item properties. The report is in CSV format. The headers are 'Key', 'Value', 'Path' and 'Guid' Use this inside a WriteFile process to write it to a file.

|Parameter    |Type    |Required|Summary                                                                                                                                                     |Example        |
|:-----------:|:------:|:------:|:----------------------------------------------------------------------------------------------------------------------------------------------------------:|:-------------:|
|CasePath     |`string`|☑️      |The path to the case.                                                                                                                                       |C:/Cases/MyCase|
|PropertyRegex|`string`|☑️      |The regex to search the property for.                                                                                                                       |Date           |
|SearchTerm   |`string`|☑️      |The term to search for.                                                                                                                                     |\*.txt         |
|ValueRegex   |`string`|        |An optional regex to check the value. If this is set, only values which match this regex will be returned, and only the contents of the first capture group.|(199\\d)       |

<a name="NuixImportDocumentIds"></a>
## NuixImportDocumentIds

**Unit**

*Requires Nuix Version 7.4*

*Requires NuixPRODUCTION_SET*

Imports the given document IDs into this production set. Only works if this production set has imported numbering.

|Parameter                    |Type    |Required|Summary                                                                                |Default Value|Example        |
|:---------------------------:|:------:|:------:|:-------------------------------------------------------------------------------------:|:-----------:|:-------------:|
|AreSourceProductionSetsInData|`bool`  |☑️      |Specifies that the source production set name(s) are contained in the document ID list.|false        |               |
|CasePath                     |`string`|☑️      |The path to the case.                                                                  |             |C:/Cases/MyCase|
|DataPath                     |`string`|☑️      |Specifies the file path of the document ID list.                                       |             |               |
|ProductionSetName            |`string`|☑️      |The production set to add results to.                                                  |             |               |

<a name="NuixMigrateCase"></a>
## NuixMigrateCase

**Unit**

*Requires Nuix Version 7.2*

Migrates a case to the latest version if necessary.

|Parameter|Type    |Required|Summary              |Example        |
|:-------:|:------:|:------:|:-------------------:|:-------------:|
|CasePath |`string`|☑️      |The path to the case.|C:/Cases/MyCase|

<a name="NuixPerformOCR"></a>
## NuixPerformOCR

**Unit**

*Requires Nuix Version 7.6*

*Requires NuixOCR_PROCESSING*

Performs optical character recognition on files in a NUIX case.

|Parameter     |Type    |Required|Summary                                                                                   |Default Value                                                                                                                                      |Example                    |Requirements|
|:------------:|:------:|:------:|:----------------------------------------------------------------------------------------:|:-------------------------------------------------------------------------------------------------------------------------------------------------:|:-------------------------:|:----------:|
|CasePath      |`string`|☑️      |The path to the case.                                                                     |                                                                                                                                                   |C:/Cases/MyCase            |            |
|OCRProfileName|`string`|        |The name of the OCR profile to use. This cannot be set at the same time as OCRProfilePath.|The default profile will be used.                                                                                                                  |MyOcrProfile               |            |
|OCRProfilePath|`string`|        |Path to the OCR profile to use. This cannot be set at the same times as OCRProfileName.   |The default profile will be used.                                                                                                                  |C:\\Profiles\\MyProfile.xml|Nuix 7.6    |
|SearchTerm    |`string`|☑️      |The term to use for searching for files to OCR.                                           |NOT flag:encrypted AND ((mime-type:application/pdf AND NOT content:\*) OR (mime-type:image/\* AND ( flag:text_not_indexed OR content:( NOT \* ) )))|                           |            |

<a name="NuixRemoveFromProductionSet"></a>
## NuixRemoveFromProductionSet

**Unit**

*Requires Nuix Version 5.0*

*Requires NuixPRODUCTION_SET*

Removes particular items from a Nuix production set.

|Parameter        |Type    |Required|Summary                                                   |Default Value             |Example        |
|:---------------:|:------:|:------:|:--------------------------------------------------------:|:------------------------:|:-------------:|
|CasePath         |`string`|☑️      |The path to the case.                                     |                          |C:/Cases/MyCase|
|ProductionSetName|`string`|☑️      |The production set to remove results from.                |                          |               |
|SearchTerm       |`string`|        |The search term to use for choosing which items to remove.|All items will be removed.|Tag:sushi      |

<a name="NuixReorderProductionSet"></a>
## NuixReorderProductionSet

**Unit**

*Requires Nuix Version 5.2*

*Requires NuixPRODUCTION_SET*

Reorders and renumbers the items in a production set.

|Parameter        |Type                                             |Required|Summary                                            |Default Value|Example        |
|:---------------:|:-----------------------------------------------:|:------:|:-------------------------------------------------:|:-----------:|:-------------:|
|CasePath         |`string`                                         |☑️      |The path to the case.                              |             |C:/Cases/MyCase|
|ProductionSetName|`string`                                         |☑️      |The production set to reorder.                     |             |               |
|SortOrder        |[ProductionSetSortOrder](#ProductionSetSortOrder)|☑️      |The method of sorting items during the renumbering.|Position     |               |

<a name="NuixSearchAndTag"></a>
## NuixSearchAndTag

**Unit**

*Requires Nuix Version 5.0*

*Requires NuixANALYSIS*

Searches a NUIX case with a particular search string and tags all files it finds.

|Parameter |Type    |Required|Summary                            |Example        |
|:--------:|:------:|:------:|:---------------------------------:|:-------------:|
|CasePath  |`string`|☑️      |The path to the case.              |C:/Cases/MyCase|
|SearchTerm|`string`|☑️      |The term to search for.            |\*.txt         |
|Tag       |`string`|☑️      |The tag to assign to found results.|               |

# Processes
<a name="AppendString"></a>
## AppendString

**Unit**

Appends a string to an existing string variable.

|Parameter|Type                         |Required|Summary                   |
|:-------:|:---------------------------:|:------:|:------------------------:|
|Variable |[VariableName](#VariableName)|☑️      |The variable to append to.|
|String   |`string`                     |☑️      |The string to append.     |

<a name="ApplyBooleanOperator"></a>
## ApplyBooleanOperator

**Boolean**

Returns true if both operands are true

|Parameter|Type                               |Required|Summary                                                   |
|:-------:|:---------------------------------:|:------:|:--------------------------------------------------------:|
|Left     |`bool`                             |☑️      |The left operand. Will always be evaluated.               |
|Operator |[BooleanOperator](#BooleanOperator)|☑️      |The operator to apply.                                    |
|Right    |`bool`                             |☑️      |The right operand. Will not be evaluated unless necessary.|

<a name="ApplyMathOperator"></a>
## ApplyMathOperator

**Int32**

Applies a mathematical operator to two integers.

|Parameter|Type                         |Required|Summary               |
|:-------:|:---------------------------:|:------:|:--------------------:|
|Left     |`int`                        |☑️      |The left operand.     |
|Operator |[MathOperator](#MathOperator)|☑️      |The operator to apply.|
|Right    |`int`                        |☑️      |The right operand.    |

<a name="Array`1"></a>
## Array`1

**List<T>**

Represents an ordered collection of objects.

|Parameter|Type                     |Required|Summary                    |
|:-------:|:-----------------------:|:------:|:-------------------------:|
|Elements |IRunnableProcess<[T](#T)>|☑️      |The elements of this array.|

<a name="ArrayCount`1"></a>
## ArrayCount`1

**Int32**

Counts the elements in an array.

|Parameter|Type         |Required|Summary            |
|:-------:|:-----------:|:------:|:-----------------:|
|Array    |List<[T](#T)>|☑️      |The array to count.|

<a name="ArrayIsEmpty`1"></a>
## ArrayIsEmpty`1

**Boolean**

Checks if an array is empty.

|Parameter|Type         |Required|Summary                          |
|:-------:|:-----------:|:------:|:-------------------------------:|
|Array    |List<[T](#T)>|☑️      |The array to check for emptiness.|

<a name="AssertError"></a>
## AssertError

**Unit**

Returns success if the Test process returns an error and a failure otherwise.

|Parameter|Type         |Required|Summary             |
|:-------:|:-----------:|:------:|:------------------:|
|Test     |[Unit](#Unit)|☑️      |The process to test.|

<a name="AssertTrue"></a>
## AssertTrue

**Unit**

Returns an error if the nested process does not return true.

|Parameter|Type  |Required|Summary             |
|:-------:|:----:|:------:|:------------------:|
|Test     |`bool`|☑️      |The process to test.|

<a name="Compare`1"></a>
## Compare`1

**Boolean**

Compares two items.

|Parameter|Type                               |Required|Summary                               |
|:-------:|:---------------------------------:|:------:|:------------------------------------:|
|Left     |[T](#T)                            |☑️      |The item to the left of the operator. |
|Operator |[CompareOperator](#CompareOperator)|☑️      |The operator to use for comparison.   |
|Right    |[T](#T)                            |☑️      |The item to the right of the operator.|

<a name="Conditional"></a>
## Conditional

**Unit**

Executes a statement if a condition is true.

|Parameter  |Type         |Required|Summary                          |
|:---------:|:-----------:|:------:|:-------------------------------:|
|Condition  |`bool`       |☑️      |Whether to follow the Then Branch|
|ElseProcess|[Unit](#Unit)|        |The Else branch, if it exists.   |
|ThenProcess|[Unit](#Unit)|☑️      |The Then Branch.                 |

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

Deletes a file or folder from the file system.

|Parameter|Type    |Required|Summary                                  |
|:-------:|:------:|:------:|:---------------------------------------:|
|Path     |`string`|☑️      |The path to the file or folder to delete.|

<a name="DoesFileContain"></a>
## DoesFileContain

**Boolean**

Returns whether a file on the file system contains a particular string.

|Parameter|Type    |Required|Summary                                 |
|:-------:|:------:|:------:|:--------------------------------------:|
|Path     |`string`|☑️      |The path to the file or folder to check.|
|Text     |`string`|☑️      |The text to check for.                  |

<a name="ElementAtIndex`1"></a>
## ElementAtIndex`1

**T**

Gets the array element at a particular index.

|Parameter|Type         |Required|Summary                         |
|:-------:|:-----------:|:------:|:------------------------------:|
|Array    |List<[T](#T)>|☑️      |The array to check.             |
|Index    |`int`        |☑️      |The index to get the element at.|

<a name="FirstIndexOf"></a>
## FirstIndexOf

**Int32**

Gets the first instance of substring in a string.

|Parameter|Type    |Required|Summary               |
|:-------:|:------:|:------:|:--------------------:|
|String   |`string`|☑️      |The string to check.  |
|SubString|`string`|☑️      |The substring to find.|

<a name="FirstIndexOfElement`1"></a>
## FirstIndexOfElement`1

**Int32**

Gets the first index of an element in an array.

|Parameter|Type         |Required|Summary                 |
|:-------:|:-----------:|:------:|:----------------------:|
|Array    |List<[T](#T)>|☑️      |The array to check.     |
|Element  |[T](#T)      |☑️      |The element to look for.|

<a name="For"></a>
## For

**Unit**

Do an action for each value of a given variable in a range.

|Parameter   |Type                         |Required|Summary                                   |
|:----------:|:---------------------------:|:------:|:----------------------------------------:|
|Action      |[Unit](#Unit)                |☑️      |The action to perform repeatedly.         |
|From        |`int`                        |☑️      |The first value of the variable to use.   |
|Increment   |`int`                        |☑️      |The amount to increment by each iteration.|
|To          |`int`                        |☑️      |The highest value of the variable to use  |
|VariableName|[VariableName](#VariableName)|☑️      |The name of the variable to loop over.    |

<a name="ForEach`1"></a>
## ForEach`1

**Unit**

Do an action for each member of the list.

|Parameter   |Type                         |Required|Summary                               |
|:----------:|:---------------------------:|:------:|:------------------------------------:|
|Action      |[Unit](#Unit)                |☑️      |The action to perform repeatedly.     |
|Array       |List<[T](#T)>                |☑️      |The elements to iterate over.         |
|VariableName|[VariableName](#VariableName)|☑️      |The name of the variable to loop over.|

<a name="GetLetterAtIndex"></a>
## GetLetterAtIndex

**String**

Gets the letters that appears at a specific index

|Parameter|Type    |Required|Summary                                |
|:-------:|:------:|:------:|:-------------------------------------:|
|Index    |`int`   |☑️      |The index.                             |
|String   |`string`|☑️      |The string to extract a substring from.|

<a name="GetSubstring"></a>
## GetSubstring

**String**

Gets a substring from a string.

|Parameter|Type    |Required|Summary                                |
|:-------:|:------:|:------:|:-------------------------------------:|
|Index    |`int`   |☑️      |The index.                             |
|Length   |`int`   |☑️      |The length of the substring to extract.|
|String   |`string`|☑️      |The string to extract a substring from.|

<a name="GetVariable`1"></a>
## GetVariable`1

**T**

Gets the value of a named variable.

|Parameter   |Type                         |Required|Summary                         |
|:----------:|:---------------------------:|:------:|:------------------------------:|
|VariableName|[VariableName](#VariableName)|☑️      |The name of the variable to get.|

<a name="IncrementVariable"></a>
## IncrementVariable

**Int32**

Increment an integer variable by a particular amount

|Parameter|Type                         |Required|Summary                    |
|:-------:|:---------------------------:|:------:|:-------------------------:|
|Amount   |`int`                        |☑️      |The amount to increment by.|
|Variable |[VariableName](#VariableName)|☑️      |The variable to increment. |

<a name="JoinStrings"></a>
## JoinStrings

**String**

Join strings with a delimiter.

|Parameter|Type          |Required|Summary                      |
|:-------:|:------------:|:------:|:---------------------------:|
|Delimiter|`string`      |☑️      |The delimiter to use.        |
|List     |List<`string`>|☑️      |The elements to iterate over.|

<a name="LastIndexOf"></a>
## LastIndexOf

**Int32**

Gets the last instance of substring in a string.

|Parameter|Type    |Required|Summary               |
|:-------:|:------:|:------:|:--------------------:|
|String   |`string`|☑️      |The string to check.  |
|SubString|`string`|☑️      |The substring to find.|

<a name="LengthOfString"></a>
## LengthOfString

**Int32**

Calculates the length of the string.

|Parameter|Type    |Required|Summary                             |
|:-------:|:------:|:------:|:----------------------------------:|
|String   |`string`|☑️      |The string to measure the length of.|

<a name="Not"></a>
## Not

**Boolean**

Negation of a boolean value.

|Parameter|Type  |Required|Summary             |
|:-------:|:----:|:------:|:------------------:|
|Boolean  |`bool`|☑️      |The value to negate.|

<a name="Print`1"></a>
## Print`1

**Unit**

Prints a value to the log.

|Parameter|Type   |Required|Summary            |
|:-------:|:-----:|:------:|:-----------------:|
|Value    |[T](#T)|☑️      |The Value to Print.|

<a name="Repeat`1"></a>
## Repeat`1

**List<T>**

Creates an array by repeating an element.

|Parameter|Type   |Required|Summary                                  |
|:-------:|:-----:|:------:|:---------------------------------------:|
|Element  |[T](#T)|☑️      |The element to repeat.                   |
|Number   |`int`  |☑️      |The number of times to repeat the element|

<a name="RepeatWhile"></a>
## RepeatWhile

**Unit**

Repeat an action while the condition is met.

|Parameter|Type         |Required|Summary                                             |
|:-------:|:-----------:|:------:|:--------------------------------------------------:|
|Action   |[Unit](#Unit)|☑️      |The action to perform repeatedly.                   |
|Condition|`bool`       |☑️      |The condition to check before performing the action.|

<a name="RepeatXTimes"></a>
## RepeatXTimes

**Unit**

Repeat a process a set number of times.

|Parameter|Type         |Required|Summary                                   |
|:-------:|:-----------:|:------:|:----------------------------------------:|
|Action   |[Unit](#Unit)|☑️      |The action to perform repeatedly.         |
|Number   |`int`        |☑️      |The number of times to perform the action.|

<a name="Sequence"></a>
## Sequence

**Unit**

A sequence of steps to be run one after the other.

|Parameter|Type                           |Required|Summary                    |
|:-------:|:-----------------------------:|:------:|:-------------------------:|
|Steps    |IRunnableProcess<[Unit](#Unit)>|☑️      |The steps of this sequence.|

<a name="SetVariable`1"></a>
## SetVariable`1

**Unit**

Gets the value of a named variable.

|Parameter   |Type                         |Required|Summary                          |
|:----------:|:---------------------------:|:------:|:-------------------------------:|
|Value       |[T](#T)                      |☑️      |The value to set the variable to.|
|VariableName|[VariableName](#VariableName)|☑️      |The name of the variable to set. |

<a name="SortArray`1"></a>
## SortArray`1

**List<T>**

Reorder an array.

|Parameter|Type                   |Required|Summary             |
|:-------:|:---------------------:|:------:|:------------------:|
|Array    |List<[T](#T)>          |☑️      |The array to modify.|
|Order    |[SortOrder](#SortOrder)|☑️      |The order to use.   |

<a name="SplitString"></a>
## SplitString

**List`1**

Splits a string.

|Parameter|Type    |Required|Summary              |
|:-------:|:------:|:------:|:-------------------:|
|Delimiter|`string`|☑️      |The delimiter to use.|
|String   |`string`|☑️      |The string to split. |

<a name="StringIsEmpty"></a>
## StringIsEmpty

**Boolean**

Returns whether a string is empty.

|Parameter|Type    |Required|Summary                             |
|:-------:|:------:|:------:|:----------------------------------:|
|String   |`string`|☑️      |The string to check for being empty.|

<a name="Test`1"></a>
## Test`1

**T**

Returns one result if a condition is true and another if the condition is false.

|Parameter|Type   |Required|Summary                          |
|:-------:|:-----:|:------:|:-------------------------------:|
|Condition|`bool` |☑️      |Whether to follow the Then Branch|
|ElseValue|[T](#T)|        |The Else branch, if it exists.   |
|ThenValue|[T](#T)|☑️      |The Then Branch.                 |

<a name="ToCase"></a>
## ToCase

**String**

Converts a string to a particular case.

|Parameter|Type                 |Required|Summary                          |
|:-------:|:-------------------:|:------:|:-------------------------------:|
|Case     |[TextCase](#TextCase)|☑️      |The case to change to.           |
|String   |`string`             |☑️      |The string to change the case of.|

<a name="Trim"></a>
## Trim

**String**

Trims a string.

|Parameter|Type                 |Required|Summary                          |
|:-------:|:-------------------:|:------:|:-------------------------------:|
|Side     |[TrimSide](#TrimSide)|☑️      |The side to trim.                |
|String   |`string`             |☑️      |The string to change the case of.|

<a name="Unzip"></a>
## Unzip

**Unit**

Unzip a file in the file system.

|Parameter           |Type    |Required|Summary                                   |Default Value|
|:------------------:|:------:|:------:|:----------------------------------------:|:-----------:|
|ArchiveFilePath     |`string`|☑️      |The path to the archive to unzip.         |             |
|DestinationDirectory|`string`|☑️      |The directory to unzip to.                |             |
|OverwriteFiles      |`bool`  |☑️      |Whether to overwrite files when unzipping.|false        |

<a name="WriteFile"></a>
## WriteFile

**Unit**

Writes a file to the local file system.

|Parameter|Type    |Required|Summary                          |
|:-------:|:------:|:------:|:-------------------------------:|
|FileName |`string`|☑️      |The name of the file to write to.|
|Folder   |`string`|☑️      |The name of the folder.          |
|Text     |`string`|☑️      |The text to write.               |

# Enums
<a name="BooleanOperator"></a>
## BooleanOperator
A boolean operator.

|Name|Summary                                      |
|:--:|:-------------------------------------------:|
|None|Sentinel value.                              |
|And |Returns true if both left and right are true.|
|Or  |Returns true if either left or right is true.|

<a name="CompareOperator"></a>
## CompareOperator
An operator to use for comparison.

|Name              |Summary        |
|:----------------:|:-------------:|
|None              |Sentinel value.|
|Equals            |               |
|NotEquals         |               |
|LessThan          |               |
|LessThanOrEqual   |               |
|GreaterThan       |               |
|GreaterThanOrEqual|               |

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

<a name="MathOperator"></a>
## MathOperator
An operator that can be applied to two numbers.

|Name    |Summary                                          |
|:------:|:-----------------------------------------------:|
|None    |Sentinel value                                   |
|Add     |Add the left and right operands.                 |
|Subtract|Subtract the right operand from the left.        |
|Multiply|Multiply the left and right operands.            |
|Divide  |Divide the left operand by the right.            |
|Modulo  |Reduce the left operand modulo the right.        |
|Power   |Raise the left operand to the power of the right.|

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

<a name="SortOrder"></a>
## SortOrder
The direction to sort by.

|Name      |Summary                                                          |
|:--------:|:---------------------------------------------------------------:|
|Ascending |Sort array elements in ascending, or alphabetical order.         |
|Descending|Sort array elements in descending, or reverse-alphabetical order.|

<a name="TextCase"></a>
## TextCase
The case to convert the text to.

|Name |Summary                                                     |
|:---:|:----------------------------------------------------------:|
|Upper|All characters will be in upper case.                       |
|Lower|All characters will be in lower case.                       |
|Title|Only the first character in each word will be in upper case.|

<a name="TrimSide"></a>
## TrimSide
The side of the string to trim.

|Name |Summary                                              |
|:---:|:---------------------------------------------------:|
|Left |Removes whitespace from the left side of the string. |
|Right|Removes whitespace from the right side of the string.|
|Both |Removes whitespace from both sides of the string.    |

