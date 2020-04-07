require 'optparse'
#RenumberProductionSet
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG')
opts.on('--productionSetNameArg0 ARG')
opts.on('--sortOrderArg0 ARG')
end.parse!(into: params)
puts params

def RenumberProductionSet(utilities,pathArg,productionSetNameArg,sortOrderArg)
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



RenumberProductionSet(utilities, params[:pathArg0], params[:productionSetNameArg0], params[:sortOrderArg0])
puts '--Script Completed Successfully--'
