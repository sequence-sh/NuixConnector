[![pipeline status](https://gitlab.com/reductech/edr/connectors/nuix/badges/master/pipeline.svg)](https://gitlab.com/reductech/edr/connectors/nuix/-/commits/master)
[![coverage report](https://gitlab.com/reductech/edr/connectors/nuix/badges/master/coverage.svg)](https://gitlab.com/reductech/edr/connectors/nuix/-/commits/master)
[![Gitter](https://badges.gitter.im/reductech/edr.svg)](https://gitter.im/reductech/edr?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)

# EDR Nuix Connector

[Reductech EDR](https://gitlab.com/reductech/edr) is a collection of
libraries that automates cross-application e-discovery and forensic workflows.

The EDR Nuix Connector allows users to automate forensic workflows using
[Nuix Workstation](https://www.nuix.com/products/nuixworkstation).

This connector has Steps to:

- Creating new cases
- Ingest concordance and loose files
- Search and tag items
- Create and update item and production sets
- Extract entities
- Create reports
- Export concordance or document metadata

### [Try Nuix Connector](https://gitlab.com/reductech/edr/edr/-/releases)

Using [EDR](https://gitlab.com/reductech/edr/edr),
the command line tool for running Sequences.

## Documentation

Documentation is available here: https://docs.reductech.io

## Example Workflow

A workflow to automatically create a case, ingest evidence,
search and tag from a CSV file, create reports, and export as concordance.

```powershell
- <CaseName>          = 'Case001'
- <Investigator>      = 'John'
- <Custodian>         = 'Bill Rapp'
- <FolderName>        = 'CASE01B0001'
- <CurrentDir>        = 'D:/Cases'
- <CasePath>          = PathCombine [<CurrentDir>, 'case']
- <ExportPath>        = PathCombine [<CurrentDir>, 'export']
- <ReportsFolder>     = PathCombine [<CurrentDir>, 'reports']
- <SearchTagCSV>      = PathCombine [<CurrentDir>, 'search-tag.csv']
- <ProductionProfile> = 'ProductionProfile'
- <ProductionSet>     = 'TaggedItemsProductionSet'
- <OCRProfileName>    = 'Default'
- <ProcessingProfileName> = 'Default'
- <Evidence> = [
    (PathCombine [<CurrentDir>, 'evidence/bill_rapp_000_1_1.pst'])
  ]

# Create a Nuix case. This keeps the connection open for the rest of the Sequence.
- NuixCreateCase
    CaseName: <CaseName>
    CasePath: <CasePath>
    Investigator: <Investigator>

# Add PST file to the case
- NuixAddItem
    Custodian: <Custodian>
    Paths: <Evidence>
    FolderName: <FolderName>
    ProcessingProfileName: <ProcessingProfileName>

# Perform OCR on the
- NuixPerformOCR OCRProfileName: <OCRProfileName>

# Read a CSV file, and for each row run a search and tag the results
- ReadFromFile <SearchTagCSV>
  | FromCsv
  | ForEach (
      NuixSearchAndTag
        SearchTerm: (From <Entity> 'SearchTerm')
        Tag: (From <Entity> 'Tag')
    )

# Add all the tagged items to an ItemSet
- NuixAddToItemSet ItemSetName: 'TaggedItems' SearchTerm: 'tag:*'

# Create a production set from the tagged item set
- NuixAddToProductionSet
    ProductionProfileName: <ProductionProfile>
    ProductionSetName: <ProductionSet>
    SearchTerm: 'item-set:TaggedItems'

# Create an item/document type report
- NuixCreateReport
  | WriteToFile Path: (PathCombine [<ReportsFolder>, 'types-report.txt'])

# Create Irregular items report and export to file
- NuixCreateIrregularItemsReport
  | WriteToFile Path: (PathCombine [<ReportsFolder>, 'irregular-items.txt'])

# Create Terms list and export to file
- NuixCreateTermList
  | WriteToFile Path: (PathCombine [<ReportsFolder>, 'term-list.txt'])

# Export concordance to file
- NuixExportConcordance
    ProductionSetName: <ProductionSet>
    ExportPath: <ExportPath>
```

## Settings

To use the Nuix connector you need to add a `nuix` section to
the `connectors` section of your `appsettings.json` file.

This is an example of that section.

```json
"connectors": {
  "nuix": {
    "version": "8.8",
    "features": [
      "ANALYSIS",
      "CASE_CREATION",
      "EXPORT_ITEMS",
      "METADATA_IMPORT",
      "OCR_PROCESSING",
      "PRODUCTION_SET"
    ],
    "exeConsolePath": "C:\\Program Files\\Nuix\\Nuix 8.8\\nuix_console.exe",
    "licencesourcetype": "dongle"
  }
}
```

This table shows the available properties

| Name                  | Required | Type       | Description                                                                                         |
| --------------------- | -------- | ---------- | --------------------------------------------------------------------------------------------------- |
| version               | ✔        | `Version`  | The installed version of Nuix.                                                                      |
| features              | ✔        | `string[]` | The available Nuix features.                                                                        |
| exeConsolePath        | ✔        | `string`   | The path to the Nuix Console application.                                                           |
| release               |          | `bool`     | Releases the semi-offline licence at the end of the execution.                                      |
| signout               |          | `bool`     | Signs the user out at the end of the execution, also releasing the semi-offline licence if present. |
| licencesourcetype     |          | `string`   | Selects a licence source type (e.g. dongle, server, cloud-server) to use.                           |
| licencesourcelocation |          | `string`   | Selects a licence source if multiple are available.                                                 |
| licencetype           |          | `string`   | Selects a licence type to use if multiple are available.                                            |
| licenceworkers        |          | `integer`  | Selects the number of workers to use if the choice is available.                                    |
| ConsoleArguments      |          | `string[]` | Additional arguments to send to the Nuix console application.                                       |

## E-discovery Reduct

The Nuix Connector is part of a group of projects called
[E-discovery Reduct](https://gitlab.com/reductech/edr)
which consists of a collection of [Connectors](https://gitlab.com/reductech/edr/connectors)
and a command-line application for running Sequences, called
[EDR](https://gitlab.com/reductech/edr/edr/-/releases).
