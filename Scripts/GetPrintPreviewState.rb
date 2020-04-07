require 'optparse'
#GetPrintPreviewState
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG')
opts.on('--productionSetNameArg0 ARG')
opts.on('--expectedStateArg0 ARG')
end.parse!(into: params)
puts params

def GetPrintPreviewState(utilities,pathArg,productionSetNameArg,expectedStateArg)

    the_case = utilities.case_factory.open(pathArg)
    productionSet = the_case.findProductionSetByName(productionSetNameArg)

    if(productionSet == nil)        
        puts 'Production Set Not Found'
        the_case.close
        exit
    else 
        r = productionSet.getPrintPreviewState()
        the_case.close

        if r == expectedStateArg
            puts "Print preview state was #{r}, as expected."
        else
            puts "Print preview state was #{r}, but expected #{expectedStateArg}"
            exit
        end
    end
end



GetPrintPreviewState(utilities, params[:pathArg0], params[:productionSetNameArg0], params[:expectedStateArg0])
puts '--Script Completed Successfully--'
