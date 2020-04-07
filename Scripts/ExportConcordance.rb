require 'optparse'
#ExportConcordance
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG')
opts.on('--exportPathArg0 ARG')
opts.on('--productionSetNameArg0 ARG')
opts.on('--metadataProfileArg0 [ARG]')
end.parse!(into: params)
puts params

def ExportConcordance(utilities,pathArg,exportPathArg,productionSetNameArg,metadataProfileArg)
    the_case = utilities.case_factory.open(pathArg)

    productionSet = the_case.findProductionSetByName(productionSetNameArg)

    if productionSet == nil

        puts "Could not find production set with name '#{:productionSetNameArg.to_s}'"

    else
        batchExporter = utilities.createBatchExporter(exportPathArg)

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



ExportConcordance(utilities, params[:pathArg0], params[:exportPathArg0], params[:productionSetNameArg0], params[:metadataProfileArg0])
puts '--Script Completed Successfully--'
