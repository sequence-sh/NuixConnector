def ImportDocumentIds(pathArg, productionSetNameArg, dataPathArg)

    
    
    the_case = utilities.case_factory.open(pathArg)

    productionSet = the_case.findProductionSetByName(productionSetNameArg)

        if(productionSet == nil)        
            puts "Production Set Not Found"
        else            
            puts "Production Set Found"

            options = 
            {
                sourceProductionSetsInData: pathArg == "true",
                dataPath: dataPathArg
            }

            failedItemsCount = productionSet.importDocumentIds(options)

            if failedItemsCount == 0
                puts "All document ids imported successfully"
            else
                puts "#{failedItemsCount} items failed to import"

        end 

    the_case.close
    
    
end