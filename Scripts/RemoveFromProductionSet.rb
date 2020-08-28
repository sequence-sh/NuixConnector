#RemoveFromProductionSet

requiredNuixVersion = '4.2'
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
	opts.on('--pathArg1 [ARG]') do |o| params[:pathArg1] = o end
	opts.on('--searchArg1 [ARG]') do |o| params[:searchArg1] = o end
	opts.on('--productionSetNameArg1 [ARG]') do |o| params[:productionSetNameArg1] = o end
end.parse!


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


RemoveFromProductionSet(utilities, params[:pathArg1], params[:searchArg1], params[:productionSetNameArg1])
puts '--Script Completed Successfully--'
