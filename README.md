# Introduction

This is a project that lets you execute various processes in NUIX from outside of NUIX and construct pipelines to automate entire workflows.<br>

To do this you will need to create a settings file with details about your NUIX application and a YAML file explaining your pipeline.

The following yaml will create a case, adds evidence from a file, and then creates a report.


```yaml
- NuixCreateCase(CaseName = 'My Case', CasePath = 'C:/MyCase', Investigator = 'Sherlock Holmes')
- NuixAddItem(CasePath = 'C:/MyCase', Custodian = 'Moriarty', FolderName = 'My Folder', Path = 'C:/Data/MyFile.txt')
- WriteFile(FileName = 'Report', Folder = 'C:/Output', Text = NuixCreateReport(CasePath = 'C:/MyCase'))

```