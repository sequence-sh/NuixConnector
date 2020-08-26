#NuixImportDocumentIds(AreSourceProductionSetsInData: False)

requiredNuixVersion = '7.4'
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
	opts.on('--sourceProductionSetsInDataArg0 ARG') do |o| params[:sourceProductionSetsInDataArg0] = o end
	opts.on('--productionSetNameArg0 ARG') do |o| params[:productionSetNameArg0] = o end
	opts.on('--dataPathArg0 ARG') do |o| params[:dataPathArg0] = o end
end.parse!


def ImportDocumentIds(pathArg,sourceProductionSetsInDataArg,productionSetNameArg,dataPathArg)


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
