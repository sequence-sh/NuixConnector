# Sequence® Nuix Connector

[Sequence®](https://sequence.sh) is a collection of libraries for
automation of cross-application e-discovery and forensic workflows.

The Nuix Connector allows users to automate forensic workflows using
[Nuix Workstation](https://www.nuix.com/products/nuixworkstation)

This connector has `Steps` to:

- Create new cases
- Ingest concordance and loose files
- Search and tag or exclude items
- Create and update item and production sets
- Extract entities
- Generate reports
- Export concordance or document metadata

## Settings

To use the Nuix Connector you need to add a `settings` block to the `nuix` connector configuration in the `connectors.json` file.

### Using a license dongle

```json
{
  "Reductech.Sequence.Connectors.Nuix": {
    "id": "Reductech.Sequence.Connectors.Nuix",
    "version": "0.9.0",
    "enabled": true,
    "settings": {
      "exeConsolePath": "C:\\Program Files\\Nuix\\Nuix 9.0\\nuix_console.exe",
      "version": "9.0",
      "licencesourcetype": "dongle",
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
}
```

### Using a license server

```json
{
  "Reductech.Sequence.Connectors.Nuix": {
    "id": "Reductech.Sequence.Connectors.Nuix",
    "version": "0.9.0",
    "enabled": true,
    "settings": {
      "exeConsolePath": "C:\\Program Files\\Nuix\\Nuix 9.0\\nuix_console.exe",
      "version": "9.0",
      "licencesourcetype": "server",
      "licencesourcelocation": "myserver",
      "licencetype": "law-enforcement-desktop",
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
| environmentVariables  |          | `string[]` | Environment variables to set before running Sequence®.                                                                             |
| ignoreWarningsRegex   |          | `string`   | Regex used to ignore java warnings coming from the Nuix connection. The default values ignores warnings from Nuix Version up to 9. |
| ignoreErrorsRegex     |          | `string`   | Regex used to ignore java errors coming from the Nuix connection. The default values ignores errors from Nuix Version up to 9.     |
| licencesourcelocation |          | `string`   | Selects a licence source if multiple are available.                                                                                |
| licencesourcetype     |          | `string`   | Selects a licence source type (e.g. dongle, server, cloud-server) to use.                                                          |
| licencetype           |          | `string`   | Selects a licence type to use if multiple are available.                                                                           |
| licenceworkers        |          | `integer`  | Selects the number of workers to use if the choice is available.                                                                   |
| release               |          | `bool`     | Releases the semi-offline licence at the end of the execution.                                                                     |
| signout               |          | `bool`     | Signs the user out at the end of the execution, also releasing the semi-offline licence if present.                                |

# Documentation

https://sequence.sh

# Download

https://sequence.sh/download

# Try SCL and Core

https://sequence.sh/playground

# Package Releases

Can be downloaded from the [Releases page](https://gitlab.com/reductech/sequence/connectors/nuix/-/releases).

# NuGet Packages

Release nuget packages are available from [nuget.org](https://www.nuget.org/profiles/Sequence).
