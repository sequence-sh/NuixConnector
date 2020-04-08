#RemoveFromProductionSet

requiredNuixVersion = '5.0'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

requiredFeatures = Array['PRODUCTION_SET']
requiredFeatures.each do |feature|
	if !utilities.getLicence().hasFeature(feature)
		puts "Nuix Feature #{feature} is required but not available."
		exit
	end
end

require 'optparse'
params = {}
OptionParser.new do |opts|
	opts.on('--pathArg0 ARG') do |o| params[:pathArg0] = o end
	opts.on('--searchArg0 [ARG]') do |o| params[:searchArg0] = o end
	opts.on('--productionSetNameArg0 ARG') do |o| params[:productionSetNameArg0] = o end
end.parse!

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
