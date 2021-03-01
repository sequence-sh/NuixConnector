# EDR Nuix Connector

[Reductech EDR](https://gitlab.com/reductech/edr) is a collection of
libraries that automates cross-application e-discovery and forensic workflows.

The EDR Nuix Connector allows users to automate Forensic workflows using
[Nuix Workstation](https://www.nuix.com/products/nuixworkstation)

This connector has `Steps` to:

- Create new cases
- Ingest concordance and loose files
- Search and tag or exclude items
- Create and update item and production sets
- Extract entities
- Generate reports
- Export concordance or document metadata

### [Try Nuix Connector](https://gitlab.com/reductech/edr/edr/-/releases)

Using [EDR](https://gitlab.com/reductech/edr/edr),
the command line tool for running Sequences.

## Documentation

- Documentation is available here: https://docs.reductech.io
- A quick-start is available here: https://docs.reductech.io/edr/how-to/quick-start.html

## E-discovery Reduct

The Nuix Connector is part of a group of projects called
[E-discovery Reduct](https://gitlab.com/reductech/edr)
which consists of a collection of [Connectors](https://gitlab.com/reductech/edr/connectors)
and a command-line application for running Sequences, called
[EDR](https://gitlab.com/reductech/edr/edr/-/releases).

### SCL

[SCL](https://docs.reductech.io/edr/how-to/sequence-configuration-language.html) stands for Sequence Configuration Language, a language for automating e-discovery and forensic workflows.

## Settings

To use the Nuix Connector you need to add a `nuix` section to the `connectors` section of your `appsettings.json` file.

### Using EDR with a license dongle

```json
"connectors": {
  "nuix": {
    "exeConsolePath": "C:\\Program Files\\Nuix\\Nuix 8.8\\nuix_console.exe",
    "licencesourcetype": "dongle",
    "version": "8.8",
    "features": [
      "ANALYSIS",
      "CASE_CREATION",
      "EXPORT_ITEMS",
      "METADATA_IMPORT",
      "OCR_PROCESSING",
      "PRODUCTION_SET"
    ]
  }
}
```

### Using EDR with a license server

```json
"connectors": {
  "nuix": {
    "exeConsolePath": "C:\\Program Files\\Nuix\\Nuix 8.8\\nuix_console.exe",
    "licencesourcetype": "server",
    "licencesourcelocation": "myserver",
    "licencetype": "law-enforcement-desktop",
    "version": "8.8",
    "ConsoleArgumentsPost": [
      "-Dnuix.licence.handlers=server",
      "-Dnuix.registry.servers=myserver"
    ],
    "EnvironmentVariables": {
      "NUIX_USERNAME": "user.name",
      "NUIX_PASSWORD": "password"
    },
    "features": [
      "ANALYSIS",
      "CASE_CREATION",
      "EXPORT_ITEMS",
      "METADATA_IMPORT",
      "OCR_PROCESSING",
      "PRODUCTION_SET"
    ]
  }
}
```

### Available settings

| Name                  | Required | Type       | Description                                                                                                                        |
| --------------------- | -------- | ---------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| exeConsolePath        | ✔        | `string`   | The path to the Nuix Console application.                                                                                          |
| features              | ✔        | `string[]` | The available Nuix features.                                                                                                       |
| version               | ✔        | `Version`  | The installed version of Nuix.                                                                                                     |
| consoleArguments      |          | `string[]` | List of console arguments to append to the nuix command.                                                                           |
| consoleArgumentsPost  |          | `string[]` | List of console arguments to prepend to the nuix command.                                                                          |
| environmentVariables  |          | `string[]` | Environment variables to set before running EDR.                                                                                   |
| ignoreWarningsRegex   |          | `string`   | Regex used to ignore java warnings coming from the Nuix connection. The default values ignores warnings from Nuix Version up to 9. |
| ignoreErrorsRegex     |          | `string`   | Regex used to ignore java errors coming from the Nuix connection. The default values ignores errors from Nuix Version up to 9.     |
| licencesourcelocation |          | `string`   | Selects a licence source if multiple are available.                                                                                |
| licencesourcetype     |          | `string`   | Selects a licence source type (e.g. dongle, server, cloud-server) to use.                                                          |
| licencetype           |          | `string`   | Selects a licence type to use if multiple are available.                                                                           |
| licenceworkers        |          | `integer`  | Selects the number of workers to use if the choice is available.                                                                   |
| release               |          | `bool`     | Releases the semi-offline licence at the end of the execution.                                                                     |
| signout               |          | `bool`     | Signs the user out at the end of the execution, also releasing the semi-offline licence if present.                                |
