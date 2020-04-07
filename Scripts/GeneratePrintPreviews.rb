require 'optparse'
#GeneratePrintPreviews
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG')
opts.on('--productionSetNameArg0 ARG')
end.parse!(into: params)
puts params

def GeneratePrintPreviews(utilities,pathArg,productionSetNameArg)
    
    the_case = utilities.case_factory.open(pathArg)

    productionSet = the_case.findProductionSetByName(productionSetNameArg)

    if(productionSet == nil)        
        puts "Production Set Not Found"
    else            
        puts "Production Set Found"

        options = {}

        resultMap = productionSet.generatePrintPreviews(options)

        puts "Print previews generated"
    end 

    the_case.close
end



GeneratePrintPreviews(utilities, params[:pathArg0], params[:productionSetNameArg0])
puts '--Script Completed Successfully--'
