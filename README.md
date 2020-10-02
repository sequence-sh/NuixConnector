[![pipeline status](https://gitlab.com/reductech/edr/connectors/nuix/badges/master/pipeline.svg)](https://gitlab.com/reductech/edr/connectors/nuix/-/commits/master)
[![coverage report](https://gitlab.com/reductech/edr/connectors/nuix/badges/master/coverage.svg)](https://gitlab.com/reductech/edr/connectors/nuix/-/commits/master)
[![Gitter](https://badges.gitter.im/reductech/edr.svg)](https://gitter.im/reductech/edr?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)


# Introduction

This is a project that lets you execute various processes in NUIX from outside of NUIX and construct pipelines to automate entire workflows.<br>

To do this you will need to create a settings file with details about your NUIX application and a YAML file explaining your pipeline.

### Running a sequence

To run a sequence, use the Nuix.Console application

`EDRNuixConsole.exe execute -p C:/MySequence.yaml`


### Yaml

Yaml is a human-readable data-serialization language you can use to define your pipelines.<br>
You define a list of steps and their parameters and they will be performed in order. <br>
For a list of possible steps see the [Documentation](documentation.md)<br>

The following yaml does the following
- Create a new case
- Add an item to the case
- Write a report on the case
- Perform OCR on relevant files
- Assign a tag to all items matching a particular search term
- Add all tagged items to a new item set
- Add everything in that item set to a new production set
- Export that production set as concordance


```yaml
- NuixCreateCase(CaseName = 'My Case', CasePath = 'C:/MyCase', Investigator = 'Sherlock Holmes')
- NuixAddItem(CasePath = 'C:/MyCase', Custodian = 'Moriarty', FolderName = 'My Folder', Path = 'C:/Data/MyFile.txt')
- WriteFile(FileName = 'Report', Folder = 'C:/Output', Text = NuixCreateReport(CasePath = 'C:/MyCase'))
- NuixPerformOCR(CasePath = 'C:/MyCase', OCRProfileName = 'My OCR Profile', SearchTerm = 'NOT flag:encrypted AND ((mime-type:application/pdf AND NOT content:*) OR (mime-type:image/* AND ( flag:text_not_indexed OR content:( NOT * ) )))')
- NuixSearchAndTag(CasePath = 'C:/MyCase', SearchTerm = 'Diamond', Tag = 'Gems')
- NuixAddToItemSet(CasePath = 'C:/MyCase', ItemSetName = 'TaggedItems', SearchTerm = 'Tag:*')
- NuixAddToProductionSet(CasePath = 'C:/MyCase', ProductionSetName = 'TaggedItemsProductionSet', SearchTerm = 'ItemSet:TaggedItems')
- NuixExportConcordance(CasePath = 'C:/MyCase', ExportPath = 'C:/Export', ProductionSetName = 'TaggedItemsProductionSet')

```

### Settings

You need to edit the App.config file to match your Nuix configuration<br>

Adjust the following settings<br>

- NuixUseDongle : Whether the Nuix authentication is supplied by a dongle
- NuixExeConsolePath : The path to the nuix console application
- NuixVersion: The version of that is installed
- NuixFeatures: The Nuix features you have available

The file should look like this

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
  <appSettings>
    <add key="NuixUseDongle" value="true"/>
    <add key="NuixExeConsolePath" value="C:\Program Files\Nuix\Nuix 8.2\nuix_console.exe"/>
    <add key="NuixVersion" value="8.2"/>
    <add key="NuixFeatures" value="ANALYSIS,CASE_CREATION,EXPORT_ITEMS,METADATA_IMPORT,OCR_PROCESSING,PRODUCTION_SET"/>
  </appSettings>
</configuration>
```


