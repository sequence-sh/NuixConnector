# Introduction

This is a project that lets you execute various processes in NUIX from outside of NUIX.

There is a console app which runs all the processes individually and you can also provide yaml containing a sequence of processes to perform.

The following yaml will create a case, add evidence from both a file and a concordance, tag some of the evidence and move it to a production set and then export the production set


```yaml
!Sequence
Steps:
- !NuixCreateCase
  CaseName: MyCase
  CasePath: &casePath C:/Cases/MyCase
  Investigator: Taj
  Description: Case Description
- !NuixAddFile
  FilePath: C:/Evidence/CaseEvidence
  Custodian: Custodian
  Description: Description
  FolderName: CaseEvidence
  CasePath: *casePath
  ProcessingProfileName: Default
- !NuixCreateReport
  OutputFolder: C:/Reports/MyCase
  CasePath: *casePath
- !NuixPerformOCR
  CasePath: *casePath
  OCRProfileName: Default
- !ForEach
  Enumeration: !CSVEnumeration
    FilePath: C:/TermsAndTags.csv
    Delimiter: ','
    HeaderInjections:
    - Header: TermToSeach
      PropertyToInject: SearchTerm
    - Header: TagToApply
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
  ProductionSetName:&productionSetName ItemsToExport
  SearchTerm: ItemSet:TaggedItems
  CasePath: *casePath
  Description: Production Set Description
- !NuixExportConcordance
  MetadataProfileName: Default
  ProductionSetName: *productionSetName
  ExportPath: c:/Exports
  CasePath: *casePath

```