# v0.13.0 (2022-01-16)

## Issues Closed in this Release

### Maintenance

- Rename EDR to Sequence #220
- Update Core to latest version #217

# v0.12.0 (2021-11-26)

Maintenance release - dependency updates only.

# v0.11.0 (2021-09-16)

Bug fixes and dependency updates.

## Issues Closed in this Release

### Bug Fixes

- NuixAddItem 'undefined method' error when mime type does not exist #199
- Do not include Core runtime assets #204
- Integration Tests are Failing #203
- Bug: Integration tests are not being run #200

# v0.10.0 (2021-07-02)

## Issues Closed in this Release

### Maintenance

- Update Core to latest and remove SCLSettings #195

# v0.9.0 (2021-05-14)

## Summary of Changes

### Core SDK

- Connector can now be used as a plugin for EDR

## Issues Closed in this Release

### New Features

- Use new settings #192

### Maintenance

- Enable publish to connector registry #193
- Update Core dependecies #191

### Other

- Allow this package to be used as a plugin #190

# v0.8.0 (2021-04-08)

## Summary of Changes

### Steps

- Added
  - `NuixGetVersion`
- Updated
  - `NuixAddConcordance`
    - Added new parameters
      - OpticonPath
      - ProcessingSettings
      - ProgressInterval
      - CustomMetadata
      - ContainerEncoding
      - ContainerLocale
      - ContainerTimeZone
    - These parameters are now optional
      - ConcordanceDateFormat
      - Description
      - Custodian
    - Added progress updates when processing

## Issues Closed in this Release

### New Features

- Allow for greater customisation of processing options in NuixAddConcordance #182
- Add step to return the nuix version #188

### Bug Fixes

- Mime type settings have no effect in NuixAddItem #186
- Return Nuix case details integration test fails on Nuix 7.0 #184

### Maintenance

- Decrease time required for nuix integration tests #177
- Improve Integration Short coverage by adding additional steps #179

# v0.7.0 (2021-03-26)

## Summary of Changes

### Steps

- Added
  - `NuixGetCaseDetails`
  - `NuixGetLicenseDetails`
- Updated
  - `NuixCreateNRTReport` to support additional report context options

## Issues Closed in this Release

### New Features

- Pass report options to CreateNRTReport as an Entity #181
- Create process to check license details, so Technicians can set the number of workers dynamically #68
- Add before and after export callbacks to ExportConcordance #180

# v0.6.0 (2021-03-14)

## Summary of Changes

### Steps

- Added
  - `NuixExcludeDecryptedItems`
  - `NuixGetAuditedSize`
- Updated
  - NuixAddItem - added Encoding, TimeZone and Locale parameters
  - NuixAddToProductionSet - production profile is now optional and can be specified using a step Parameter
  - NuixExportConcordance - the export file types and concordance type can now be configured using Step parameters

## Issues Closed in this Release

### New Features

- Add additional configuration to NuixExportConcordance #174
- Pass profile configuration as an Entity to AddToProductionSet #173
- Add a step to exclude decrypted items #170
- Add a step to calculate the total size for a case #169
- Add ItemSort parameter to AddToProductionSet #172
- Add SearchType parameter to AddToProductionSet and AddToItemSet #171
- NuixAddItem Custodian parameter should be optional #167
- Add Encoding, TimeZone and Locale parameters to NuixAddItem #168
- Add helper ruby functions to improve maintanability #162
- Add log localization #166

# v0.5.0 (2021-03-01)

## Summary of Changes

### Steps

- Added `RemoveFromItemSet` step
- Added `SearchAndExclude` step
- Added `SearchType` parameter to `SearchAndTag` and `SearchAndExclude` steps which allows users to search for item descendants, duplicates, families and threads

### Connector Updates

- Connector is now compatible with Nuix 9.0
- Steps that search for items (e.g. `AddToItemSet`, `CountItems`, etc.) now take parameters with search options. Performance has been optimised to use unsorted search by default, with the option to use a sorted search if required
- The arguments sent to the Nuix console function are now fully configurable. This allows you to use any type of authentication.

## Issues Closed in this Release

### New Features

- Add additional logging to steps #153
- Allow AddItem step to add custom metadata to evidence containers #156
- Add the ability to pass search options to steps #157
- Add step to remove items from an item set #159
- Add step to search for items and exclude them from a case #160
- Allow technicians to perform descendant and family searches #161
- Add details to messages logged from NuixConnection #158
- Disable Trace Logging for integration testing to prevent exposure of secrets #149
- Allow error and warning messages received from nuix to be ignored #148
- Allow License Server Authentication #147

### Bug Fixes

- Nuix connection not closed on exit #155
- NuixReorderProductionSet should convert enum to ruby value #150
- Export Concordance (9.0) integration test fails trying to delete concordance directory #152

### Maintenance

- Update nuix connector script to v0.2.1 #163
- Run unit tests along with integration #151
- Split integration testing into two pipelines - full and short #145
- Ensure compatibility and add integration testing for Nuix 9 #144

## v0.4.0 (2021-01-29)

- Upgrade to Core v0.4.0 and the new SCL configuration language
- Add step to run inline ruby scripts
- The connector script is now a separate project
- Reworked logging and exceptions

### New Features

- Use dynamic settings #138
- Remove Version checking code as that is now handled by core #135
- Use SourceGeneration for tests #133
- Split out nuix connector ruby script into a separate project and add unit tests #104
- Change Ruby Script so that it doesn't automatically close cases #130
- Create OpenCase and CloseCase steps #121
- Update Logging and Error Messages to support latest version of Core #129
- Add parameter and step aliases, to make SCL more user-friendly #123
- Update Core and refactor to use AsyncList instead of EntityStreams and Arrays #126
- Update version of Core to support new language features #122
- Create RunRubyScript step so technicians can run arbitrary scripts in Nuix #116
- Upgrade to .NET 5 #117

### Bug Fixes

- Unable to add or update the Reductech.EDR.Connectors.Nuix nuget package #137
- Nuix Connector no longer supports versions 6 and 7 #103
- Scoped state does not persist nuix connection #124
- Running from EDR should not result in "Unexpected character encountered while parsing value" error #114
- Integration test job should produce a coverage report #111
- Fix flaky Integration Tests #118

### Maintenance

- Update to latest version of Core #136 #139
- Re-enable integration tests for Nuix 7 #131
- Update nuixconnectorscript to v0.1.1 #134
- Add .editorconfig file and standardize formatting #127
- Include all tests in the integration job to make test coverage more representative #120
- Disable push on the nightly integration test pipeline #112

## v0.3.0 (2020-11-27)

The way the connector interacts with Nuix has been rewritten - functions
and data are now streamed to Nuix so there is no longer a requirement
for script composition.

**Breaking Change**: this version currently only supports Nuix 8.

### New Features

- Stream functions and data instead of script composition #102
- Support Entity arguments for Nuix scripts, to allow Technicians to customise processing #99

### Maintenance

- Update to latest version of Core and use new property names #109
- Update to latest version of core to take advantage of Streams, Entities, and EntityStreams #100
- Add Release issue template #98
- Use template ci config, so that it's easier to maintain #97

## v0.2.1 (2020-11-03)

### New Features

- Use Step test library for unit testing to improve error detection #93
- Remove Console App #86

### Bug Fixes

- Optional parameters set as required #91
- Parameters with default values should not have the required attribute #89
- Add job to package exe on master branch #85

### Other

- Update NUIX versions for integration testing #92
- Update version of core and add support for new error handling #90
- Update to use latest version of Core #87

## v0.2.0 (2020-10-02)

Updated to use v0.2.0 of reductech/edr/core>

The yaml specification has changed entirely so yaml from the previous version will not work with this version.

### New Features

- Allow composition of ruby processes #64
- Convert to new process Paradigm #62
- Any Nuix process that uses a profile should take a profile name OR a path to a file argument #52
- Add support for password lists in Nuix Add Item #50
- Add support for password lists in Nuix Add Item #50
- Regular and Compatibility methods should be merged #49
- Create a process for changing item custodians in Nuix #51
- Loop should be able to take the output of a process as an Enumerator #47
- Allow users of Nuix v5 to v7.5 Perform OCR and add items #42
- Make Nuix Reporting a consistent experience #41
- Nuix version number and features should be tested every time a script is run #46
- Make Scripts work on Nuix 6 and 7 #45
- Set up integration tests for older versions of NUIX #43
- Autogenerate Ruby Scripts #44
- Add Required Nuix versions and Features to all Nuix methods #33

### Bug Fixes

- Fix regression caused by changing "processor.process" to "processor.step" #82
- Merge processes and Processes directories into one #72
- Integration Tests cannot acquire license #67
- Bug: Summaries for properties in referenced packages do not appear in documentation #61
- Nuix Export Concordance should extract naming conventions from the chosen profile #53
- Nuix OCR should give a better error message if the profile cannot be found #48

### Maintenance

- Integrate new CI/CD script and project properties #74
- Use Testing library #81
- Rename NuixTests to Nuix.Tests #79
- Rename 'Process' to 'Step' #80
- Correct Mistakes in attribute Names #77
- Update Processes package to latest version #75
- Remove processes submodule from solution directory #73
- Update to latest version of processes #71
- Add issue templates #66
- Remove Processes submodule #65

### Documentation

- Add badges to readme #84
- Create a readme so users can see what the project is about and get started #78
- Update Documentation - add Requirements #55

### Other

- Create Script Argument Tests, to ensure scripts only try to access existing variables #70
- Update unit tests to xUnit #63
- Adjust Namespaces for Nuix #60
- Switch to dynamic process finder #59
- Update to use the new version of Processes #57

## v0.1.0 (2020-03-13)

### New Features

- Get ruby scripts to output unicode characters #35
- Workflow - Identify Candiates for OCR Processing #25
- Add a type of process that iterates over files in a folder and executes a nested process #24
- Add OCR functionality #13
- Create Reports #22
- Add Support for item groups #21
- Try yaml instead of json #20
- Create JSON schema for orchestrating tasks #18
- Create API #17
- Create Console App #16
- Export concordance load file from NUIX #6
- Add concordance data to a NUIX case #3
- Add a File\Folder\Image to a NUIX case #4


