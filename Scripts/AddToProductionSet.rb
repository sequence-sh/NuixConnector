require 'optparse'
#AddToProductionSet
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG')
opts.on('--searchArg0 ARG')
opts.on('--productionSetNameArg0 ARG')
opts.on('--descriptionArg0 [ARG]')
opts.on('--orderArg0 [ARG]')
opts.on('--limitArg0 [ARG]')
end.parse!(into: params)
puts params

def AddToProductionSet(utilities,pathArg,searchArg,productionSetNameArg,descriptionArg,orderArg,limitArg)
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



AddToProductionSet(utilities, params[:pathArg0], params[:searchArg0], params[:productionSetNameArg0], params[:descriptionArg0], params[:orderArg0], params[:limitArg0])
puts '--Script Completed Successfully--'
