require 'optparse'
#AnnotateDocumentIds
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG')
opts.on('--productionSetNameArg0 ARG')
opts.on('--dataPathArg0 ARG')
end.parse!(into: params)
puts params

def AnnotateDocumentIds(utilities,pathArg,productionSetNameArg,dataPathArg)

    the_case = utilities.case_factory.open(pathArg)
    productionSet = the_case.findProductionSetByName(productionSetNameArg)

    if(productionSet == nil)        
        puts "Production Set Not Found"
    else            
        puts "Production Set Found"

        options = 
        {
            dataPath: dataPathArg
        }
        resultMap = productionSet.annotateDocumentIdList(options)
        puts resultMap
    end 

    the_case.close
end



AnnotateDocumentIds(utilities, params[:pathArg0], params[:productionSetNameArg0], params[:dataPathArg0])
puts '--Script Completed Successfully--'
