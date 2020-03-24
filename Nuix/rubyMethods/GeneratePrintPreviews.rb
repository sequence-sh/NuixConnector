def GeneratePrintPreviews(pathArg, productionSetNameArg)

    
    
    the_case = utilities.case_factory.open(pathArg)

    productionSet = the_case.findProductionSetByName(productionSetNameArg)

        if(productionSet == nil)        
            puts "Production Set Not Found"
        else            
            puts "Production Set Found"

            options = {}

            resultMap = productionSet.generatePrintPreviews(options)

            puts "Print previews generated"
        end 

    the_case.close
    
    
end