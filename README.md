# Introduction

This is a project that lets you execute various processes in NUIX from outside of NUIX.

There is a console app which runs all the processes individually and you can also provide yaml containing a sequence of processes to perform.

The following yaml will create a case, add evidence from both a file and a concordance, tag some of the evidence and move it to a production set and then export the production set


```yaml
!Sequence
Steps:
- !NuixCreateCase
  CaseName: Case Name
  CasePath: &casePath Case Path
  Investigator: Investigator
  Description: Case Description
- !NuixAddFile
  FilePath: File Path
  Custodian: Custodian
  Description: Description
  FolderName: Folder Name
  CasePath: *casePath
  ProcessingProfileName: Default
- !NuixCreateReport
  OutputFolder: Report Output Folder
  CasePath: *casePath
- !NuixPerformOCR
  CasePath: *casePath
  OCRProfileName: OCR Profile
- !ForEach
  Enumeration: !CSVEnumeration
    FilePath: *casePath
    Delimiter: ','
    HeaderInjections:
    - Header: SearchTerm
      PropertyToInject: SearchTerm
    - Header: Tag
      PropertyToInject: Tag
    HasFieldsEnclosedInQuotes: false
    RemoveDuplicates: false
  SubProcess: !NuixSearchAndTag
    CasePath: *casePath
- !NuixAddToItemSet
  ItemSetName: TaggedItems
  SearchTerm: Tag:*
  CasePath: *casePath
  ItemSetDeduplication: Default
  DeduplicateBy: Individual
- !NuixAddToProductionSet
  ProductionSetName: Production Set Name
  SearchTerm: ItemSet:TaggedItems
  CasePath: *casePath
  Description: Production Set Description`
- !NuixExportConcordance
  MetadataProfileName: Default
  ProductionSetName: Production Set Name
  ExportPath: Export Path
  CasePath: *casePath

```