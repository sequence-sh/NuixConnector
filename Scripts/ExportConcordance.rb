#ExportConcordance

requiredNuixVersion = '5.0'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

requiredFeatures = Array['EXPORT_ITEMS', 'PRODUCTION_SET']
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
	opts.on('--exportPathArg0 ARG') do |o| params[:exportPathArg0] = o end
	opts.on('--productionSetNameArg0 ARG') do |o| params[:productionSetNameArg0] = o end
	opts.on('--metadataProfileArg0 [ARG]') do |o| params[:metadataProfileArg0] = o end
	opts.on('--productionProfileArg0 [ARG]') do |o| params[:productionProfileArg0] = o end
end.parse!

puts params


def ExportConcordance(utilities,pathArg,exportPathArg,productionSetNameArg,metadataProfileArg,productionProfileArg)

    the_case = utilities.case_factory.open(pathArg)

    productionSet = the_case.findProductionSetByName(productionSetNameArg)

    if productionSet == nil
        puts "Could not find production set with name '#{productionSetNameArg.to_s}'"
    else
        batchExporter = utilities.createBatchExporter(exportPathArg)

        if(productionProfileArg != nil)
            batchExporter.setProductionProfile(productionProfileArg)
        end


        batchExporter.addLoadFile("concordance",{
        :metadataProfile => metadataProfileArg
		})

        batchExporter.addProduct("native", {
        :naming=> "full",
        :path => "Native"
        })

        batchExporter.addProduct("text", {
        :naming=> "full",
        :path => "Text"
        })


        puts 'Starting export.'
        batchExporter.exportItems(productionSet)        
        puts 'Export complete.'

    end

    the_case.close
end



ExportConcordance(utilities, params[:pathArg0], params[:exportPathArg0], params[:productionSetNameArg0], params[:metadataProfileArg0], params[:productionProfileArg0])
puts '--Script Completed Successfully--'
