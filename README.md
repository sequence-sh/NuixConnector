# Introduction

This is a project that lets you execute various processes in NUIX from outside of NUIX.

There is a console app which runs all the processes individually and you can also provide yaml containing a sequence of processes to perform.

The following yaml will create a case, add evidence from both a file and a concordance, tag some of the evidence and move it to a production set and then export the production set


```yaml
!Sequence
Steps:
- !NuixCreateCase
  CaseName: Case Name
  CasePath: &CasePath Case Path
  Investigator: Investigator
- !NuixAddItem
  Path: File Path
  Custodian: Custodian
  FolderName: Folder Name
  CasePath: *CasePath
- !NuixCreateReport
  OutputFolder: Report Output Folder
  CasePath: *CasePath
- !NuixPerformOCR
  CasePath: *CasePath
  OCRProfileName: OCR Profile
- !Loop
  For: !CSV
    CSVFilePath: CSV Path
    InjectColumns:
      SearchTerm:
        Property: SearchTerm
      Tag:
        Property: Tag
    Delimiter: ','
    HasFieldsEnclosedInQuotes: false
  RunProcess: !NuixSearchAndTag
    CasePath: *CasePath
- !NuixAddToItemSet
  ItemSetName: TaggedItems
  SearchTerm: Tag:*
  CasePath: *CasePath
- !NuixAddToProductionSet
  ProductionSetName: &ProductionSetName Production Set Name
  SearchTerm: ItemSet:TaggedItems
  CasePath: *CasePath
- !NuixExportConcordance
  MetadataProfileName: Default
  ProductionSetName: *ProductionSetName
  ExportPath: Export Path
  CasePath: *CasePath


```