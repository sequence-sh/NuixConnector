def AddToCase(pathArg, processingProfileNameArg, folderNameArg, folderDescriptionArg, folderCustodianArg, filePathArg)

    
    
    the_case = utilities.case_factory.open(pathArg)

    processor = the_case.create_processor
    processor.setProcessingProfile(processingProfileNameArg) if processingProfileNameArg != nil

    folder = processor.new_evidence_container(folderNameArg)

    folder.description = folderDescriptionArg if folderDescriptionArg != nil
    folder.initial_custodian = folderCustodianArg

    folder.add_file(filePathArg)
    folder.save

    puts 'Starting processing.'
    processor.process
    puts 'Processing complete.'
    the_case.close
    
    
end