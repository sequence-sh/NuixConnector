def RemoveFromProductionSet(pathArg, productionSetNameArg, searchArg)

    
    
    the_case = utilities.case_factory.open(pathArg)

    puts "Searching"

        productionSet = the_case.findProductionSetByName(productionSetNameArg)

        if(productionSet == nil)
            puts "Production Set Not Found"
        else
            puts "Production Set Found"

            if searchArg != nil
                items = the_case.searchUnsorted(searchArg)
                productionSetItems = productionSet.getItems();
                itemsToRemove = items.to_a & productionSetItems
                productionSet.removeItems(itemsToRemove)
                puts "#{itemsToRemove.length} removed"

            else
                previousTotal = getItems().length

                productionSet.removeAllItems()
                puts "All items (#{previousTotal}) removed"
            end

            

        end

    the_case.close
    
    
end