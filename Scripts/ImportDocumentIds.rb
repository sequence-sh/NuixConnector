require 'optparse'
#ImportDocumentIds
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG')
opts.on('--sourceProductionSetsInDataArg0 ARG')
opts.on('--productionSetNameArg0 ARG')
opts.on('--dataPathArg0 ARG')
end.parse!(into: params)
puts params

def ImportDocumentIds(utilities,pathArg,sourceProductionSetsInDataArg,productionSetNameArg,dataPathArg)
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



ImportDocumentIds(utilities, params[:pathArg0], params[:sourceProductionSetsInDataArg0], params[:productionSetNameArg0], params[:dataPathArg0])
puts '--Script Completed Successfully--'
