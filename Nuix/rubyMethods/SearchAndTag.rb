def SearchAndTag(pathArg, searchArg, orderArg, limitArg, tagArg)

    
    
    the_case = utilities.case_factory.open(pathArg)

    puts "Searching for '#{searchArg}'"

    searchOptions = {}
    searchOptions[:order] = orderArg if orderArg != nil
    searchOptions[:limit] = limitArg.to_i if limitArg != nil


    items = the_case.search(searchArg, searchOptions)

    puts "#{items.length} found"

    j = 0

    items.each {|i|
       added = i.addTag(tagArg)
       j += 1 if added
    }

    puts "#{j} items tagged"

    the_case.close
    
    
end