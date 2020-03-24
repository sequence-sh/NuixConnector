def AnnotateDocumentIds(pathArg, productionSetNameArg, dataPathArg)

    
    
    the_case = utilities.case_factory.open(pathArg)

    productionSet = the_case.findProductionSetByName(productionSetNameArg)

        if(productionSet == nil)        
            puts "Production Set Not Found"
        else            
            puts "Production Set Found"

            options = 
            {
                dataPath: dataPathArg
            }

            resultMap = productionSet.annotateDocumentIdList(options)

            puts resultMap

        end 

    the_case.close
    
    
end