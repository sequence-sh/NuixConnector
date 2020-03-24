def CreateCase(pathArg, nameArg, descriptionArg, investigatorArg)

    puts "Creating Case"
    
        the_case = utilities.case_factory.create(pathArg,
            :name => nameArg,
            :description => descriptionArg,
            :investigator => investigatorArg)
    puts "Case Created"
        the_case.close
    
    
end