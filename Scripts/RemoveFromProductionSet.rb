require 'optparse'
#RemoveFromProductionSet
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG')
opts.on('--searchArg0 [ARG]')
opts.on('--productionSetNameArg0 ARG')
end.parse!(into: params)
puts params

def RemoveFromProductionSet(utilities,pathArg,searchArg,productionSetNameArg)
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



RemoveFromProductionSet(utilities, params[:pathArg0], params[:searchArg0], params[:productionSetNameArg0])
puts '--Script Completed Successfully--'
