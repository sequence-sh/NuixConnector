[![pipeline status](https://gitlab.com/reductech/edr/connectors/nuix/badges/master/pipeline.svg)](https://gitlab.com/reductech/edr/connectors/nuix/-/commits/master)
[![coverage report](https://gitlab.com/reductech/edr/connectors/nuix/badges/master/coverage.svg)](https://gitlab.com/reductech/edr/connectors/nuix/-/commits/master)
[![Gitter](https://badges.gitter.im/reductech/edr.svg)](https://gitter.im/reductech/edr?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge)
 
# EDR Nuix Connector
 
[Reductech EDR](https://gitlab.com/reductech/edr) is a collection of
libraries that automates cross-application e-discovery and forensic workflows.
 
The EDR Nuix Connector allows users to automate Forensic workflows using
[Nuix Workstation](https://www.nuix.com/products/nuixworkstation)
 
The Connector allows:
 
-
 
### [Try Nuix Connector](https://gitlab.com/reductech/edr/edr/-/releases)
 
## Documentation
 
- Documentation is available here: https://docs.reductech.io
- A quick-start is available here: https://docs.reductech.io/howto/quick-start.html
- Developers documentation is available here: https://docs.reductech.io/developers/intro.html
 
## E-discovery Reduct
 
Core is part of a group of projects called
[E-discovery Reduct](https://gitlab.com/reductech/edr)
which consists of a collection of [Connectors](https://gitlab.com/reductech/edr/connectors)
and a command-line application for running Sequences, called
[EDR](https://gitlab.com/reductech/edr/edr/-/releases).

### SCL

[SCL](https://docs.reductech.io/howto/scl.html) stands for Sequence Configuration Language, a language for automating e-discovery and forensic workflows.

### Nuix

The [Nuix Engine](https://www.nuix.com/) combines load balancing, fault tolerance and processing technologies to provide insights from large volumes of unstructured, semi-structured and structured data

### [Examples](Link Here)



### Settings

To use the Nuix connector you need to add a `nuix` section to the `connectors` section of your `appsettings.json` file.

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

|Name |Required |Type |Description |
|-|-|-|-|
|version |✔ |`Version` |The installed version of Nuix. |
|features |✔ |`string[]` |The available Nuix features. |
|exeConsolePath|✔ |`string` |The path to the Nuix Console application. |
|release || `bool`| Releases the semi-offline licence at the end of the execution.|
|signout || `bool`| Signs the user out at the end of the execution, also releasing the semi-offline licence if present.|
|licencesourcetype | |`string` |Selects a licence source type (e.g. dongle, server, cloud-server) to use. |
|licencesourcelocation||`string` |Selects a licence source if multiple are available. |
|licencetype | |`string` |Selects a licence type to use if multiple are available. |
|licenceworkers | |`integer` |Selects the number of workers to use if the choice is available. |
|ConsoleArguments | |`string[]` |Additional arguments to send to the Nuix console application. |
