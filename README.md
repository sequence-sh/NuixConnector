# Introduction

This is a project that lets you execute various processes in NUIX from outside of NUIX.

There is a console app which runs all the processes individually and you can also provide JSON containing a sequence of processes to perform.

The following JSON will create a case, add evidence from both a file and a concordance, tag some of the evidence and move it to a production set and then export the production set


```json
{
	"Type": "MultiStepProcess",
	"Steps": [
		{
			"Type": "CreateCaseProcess",
			"CaseName": "My Case",
			"CasePath": "C:/Cases/MyCase",
			"Investigator": "Mark",
			"Description": "My new case"
		},
		{
			"Type": "AddFileProcess",
			"Conditions": [
				{
					"FilePath": "C:/MyFolder",
					"Type": "FileExistsCondition"
				}
			],
			"FilePath": "C:/MyFolder",
			"Custodian": "Mark",
			"Description": "Evidence from file",
			"FolderName": "Evidence Folder 1",
			"CasePath": "C:/Cases/MyCase",
			"ProcessingProfileName": null
		},
		{
			"Type": "AddConcordanceProcess",
			"Conditions": [
				{
					"FilePath": "C:/MyConcordance.dat",
					"Type": "FileExistsCondition"
				}
			],
			"ConcordanceProfileName": "Default",
			"ConcordanceDateFormat": "yyyy-MM-dd'T'HH:mm:ss.SSSZ",
			"FilePath": "C:/MyConcordance.dat",
			"Custodian": "Mark",
			"Description": "Evidence from concordance",
			"FolderName": "Evidence Folder 2",
			"CasePath": "C:/Cases/MyCase"
		},
		{
			"Type": "SearchAndTagProcess",
			"Tag": "Dinosaurs",
			"SearchTerm": "Raptor",
			"CasePath": "C:/Cases/MyCase"
		},
		{
			"Type": "AddToProductionSetProcess",
			"ProductionSetName": "Dinosaurs",
			"SearchTerm": "Raptor",
			"CasePath": "C:/Cases/MyCase"
		},
		{
			"Type": "ExportConcordanceProcess",
			"MetadataProfileName": "Default",
			"ProductionSetName": "Dinosaurs",
			"ExportPath": "C:/Exports",
			"CasePath": "C:/Cases/MyCase"
		}
	]
}
```