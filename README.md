# Introduction

This is a project that lets you execute various processes in NUIX from outside of NUIX.

There is a console app which runs all the processes individually and you can also provide yaml containing a sequence of processes to perform.

The following yaml will create a case, add evidence from both a file and a concordance, tag some of the evidence and move it to a production set and then export the production set


```yaml
!Sequence
Steps:
- !NuixCreateCase
  CaseName: Case Name
  CasePath: Case Path
  Investigator: Investigator
- !NuixAddItem
  Path: File Path
  Custodian: Custodian
  FolderName: Folder Name
  CasePath: Case Path
- !NuixCreateReport
  OutputFolder: Report Output Folder
  CasePath: Case Path
- !NuixPerformOCR
  CasePath: Case Path
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
    CasePath: Case Path
- !NuixAddToItemSet
  ItemSetName: TaggedItems
  SearchTerm: Tag:*
  CasePath: Case Path
- !NuixAddToProductionSet
  ProductionSetName: Production Set Name
  SearchTerm: ItemSet:TaggedItems
  CasePath: Case Path
- !NuixExportConcordance
  MetadataProfileName: Default
  ProductionSetName: Production Set Name
  ExportPath: Export Path
  CasePath: Case Path


```