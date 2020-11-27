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
