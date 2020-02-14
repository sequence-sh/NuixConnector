# Introduction

This is a project that lets you execute various processes in NUIX from outside of NUIX.

There is a console app which runs all the processes individually and you can also provide yaml containing a sequence of processes to perform.

The following yaml will create a case, add evidence from both a file and a concordance, tag some of the evidence and move it to a production set and then export the production set


```yaml
!MultiStepProcess
Steps:
- !CreateCaseProcess
  CaseName: My Case
  CasePath: &casePath C:/Cases/MyCase
  Investigator: Mark
  Description: My new case
- !AddFileProcess
  FilePath: C:/MyFolder
  Custodian: Mark
  Description: Evidence from file
  FolderName: Evidence Folder 1
  CasePath: *casePath
  Conditions:
  - !FileExistsCondition
    FilePath: C:/MyFolder
- !AddConcordanceProcess
  ConcordanceProfileName: Default
  ConcordanceDateFormat: yyyy-MM-dd'T'HH:mm:ss.SSSZ
  FilePath: C:/MyConcordance.dat
  Custodian: Mark
  Description: Evidence from concordance
  FolderName: Evidence Folder 2
  CasePath: *casePath
  Conditions:
  - !FileExistsCondition
    FilePath: C:/MyConcordance.dat
- !SearchAndTagProcess
  Tag: Dinosaurs
  SearchTerm: Raptor
  CasePath: *casePath
- !AddToProductionSetProcess
  ProductionSetName: &productionSet Dinosaurs
  SearchTerm: Raptor
  CasePath: *casePath
- !ExportConcordanceProcess
  MetadataProfileName: Default
  ProductionSetName: *productionSet
  ExportPath: C:/Exports
  CasePath: *casePath
```