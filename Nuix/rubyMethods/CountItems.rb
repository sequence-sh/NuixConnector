def CountItems(pathArg, searchArg)

    
    
    the_case = utilities.case_factory.open(pathArg)

    puts "Counting '#{searchArg}'"

    searchOptions = {}

    count = the_case.count(searchArg, searchOptions)

    puts "#{count} found"

    the_case.close
    
    
end