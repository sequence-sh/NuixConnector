require 'optparse'
#RunOCR
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG')
opts.on('--searchTermArg0 ARG')
opts.on('--ocrProfileArg0 [ARG]')
end.parse!(into: params)
puts params

def RunOCR(utilities,pathArg,searchTermArg,ocrProfileArg)
   the_case = utilities.case_factory.open(pathArg)

            searchTerm = searchTermArg    
    
        items = the_case.searchUnsorted(searchTerm).to_a

            puts "Running OCR on #{items.length} items"
    
            processor = utilities.createOcrProcessor()


                if ocrProfileArg != nil
            ocrProfileStore = the_case.getOcrProfileStore()

        puts "Got profile store"

        profile = ocrProfileStore.getProfile(ocrProfileArg)

        if profile != nil
            processor.process(items, profile)
        puts "Items Processed"
        else
        puts "Could not find profile '#{ocrProfileArg}'"
        end
        else
        processor.process(items)
            puts "Items Processed"
        end    
        the_case.close
end



RunOCR(utilities, params[:pathArg0], params[:searchTermArg0], params[:ocrProfileArg0])
puts '--Script Completed Successfully--'
