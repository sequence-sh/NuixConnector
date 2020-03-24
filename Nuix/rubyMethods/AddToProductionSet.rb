def AddToProductionSet(pathArg, orderArg, limitArg, searchArg, productionSetNameArg, descriptionArg)

    
    
    the_case = utilities.case_factory.open(pathArg)

    puts "Searching"

    searchOptions = {}
    searchOptions[:order] = orderArg if orderArg != nil
    searchOptions[:limit] = limitArg.to_i if limitArg != nil

    items = the_case.search(searchArg, searchOptions)

    puts "#{items.length} found"

    if items.length > 0

        productionSet = the_case.findProductionSetByName(productionSetNameArg)

        if(productionSet == nil)

            options = {}
            options[:description] = descriptionArg.to_i if descriptionArg != nil

            productionSet = the_case.newProductionSet(productionSetNameArg, options)
        
            puts "Production Set Created"
        else
            puts "Production Set Found"
        end

        productionSet.addItems(items)

        puts "items added"
    end    

    the_case.close
    
    
end