/label ~backstage 
/label ~"wf::backlog" 
/label ~"area::release/packaging" 
/label ~nuix 
# Prepare for release `<insert version>`

## Before

- [ ] Make sure that the project files have the correct release version set
   - The `<Version>` element has to match the release tag (without the *v* prefix).
- [ ] Update any Reductech dependencies to release versions
   - When building releases, the CI job will only restore packages from the production
   nuget. Therefore, if a library has any pre-release dependencies, the build stage will fail.
   - To check for updates: `dotnet list .\PathTo\Project.csproj package --outdated`
   - To update: `dotnet add .\PathTo\Project.csproj package Reductech.EDR.Core`
- [ ] Update the changelog
   - Manually, or use: reductech/pwsh/New-Changelog>
   - To include all issues from the last release tag to *HEAD*:
   `New-Changelog.ps1 -ProjectId <GitLabProjectId> -ReleaseVersion <insert version>`
- [ ] Update the readme / documentation with any new changes

## Create Release

- [ ] Go to Repository > Tags > New Tag
  - Tag name: v0.1.0
  - *Message* and *Release Notes* should be the same: brief description of the release
  and any major (especially breaking) changes, and a link to the `changelog.md`.

## After

- [ ] Attach packages to the release
    - Wait for the release pipeline to finish
    - Go to Project Overview > Releases
    - Click on edit (pencil in the top-right) for the new release
    - In the *Release assets* section, add a new link for each package job artifact:
       - URL: Link to the package job artifacts. e.g. https://gitlab.com/reductech/edr/core/-/jobs/828412665/artifacts/download?file_type=archive
       - Link title: `Reductech.EDR.Core-v0.2.1.dll.zip`
       - Type: *Package*
    - Yes, this should and will be automated. Soonish.
- [ ] Increment minor version for all the projects.
   - Create a new MR
   - Update the `<Version>` element in the csproj files
   - Merge into master
