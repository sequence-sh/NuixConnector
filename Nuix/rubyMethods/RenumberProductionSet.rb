def RenumberProductionSet(pathArg, productionSetNameArg, sortOrderArg)

    
    
    the_case = utilities.case_factory.open(pathArg)

    productionSet = the_case.findProductionSetByName(productionSetNameArg)

        if(productionSet == nil)        
            puts "Production Set Not Found"
        else            
            puts "Production Set Found"

            options = 
            {
                sortOrder: sortOrderArg
            }

            resultMap = productionSet.renumber(options)

            puts resultMap

        end 

    the_case.close
    
    
end