def MigrateCase(pathArg)

    puts "Opening Case, migrating if necessary"
    
    options = {migrate: true}

    the_case = utilities.case_factory.open(pathArg, options)
    
    
end