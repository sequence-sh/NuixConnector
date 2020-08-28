#RunOCR

requiredNuixVersion = '7.6'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

requiredFeatures = Array['OCR_PROCESSING']
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
	opts.on('--searchTermArg1 [ARG]') do |o| params[:searchTermArg1] = o end
	opts.on('--ocrProfileArg1 [ARG]') do |o| params[:ocrProfileArg1] = o end
	opts.on('--ocrProfilePathArg1 [ARG]') do |o| params[:ocrProfilePathArg1] = o end
end.parse!


def RunOCR(utilities,pathArg,searchTermArg,ocrProfileArg,ocrProfilePathArg)

    the_case = utilities.case_factory.open(pathArg)

    searchTerm = searchTermArg
    items = the_case.searchUnsorted(searchTerm).to_a

    puts "Running OCR on #{items.length} items"

    processor = utilities.createOcrProcessor() #since Nuix 7.0 but seems to work with earlier versions anyway

    if ocrProfileArg != nil
        ocrOptions = {:ocrProfileName => ocrProfileArg}
        processor.process(items, ocrOptions)
        puts "Items Processed"
    elsif ocrProfilePathArg != nil
        profileBuilder = utilities.getOcrProfileBuilder()
        profile = profileBuilder.load(ocrProfilePathArg)

        if profile == nil
            puts "Could not find processing profile at #{ocrProfilePathArg}"
            exit
        end

        processor.setOcrProfileObject(profile)
    else
        processor.process(items)
        puts "Items Processed"
    end
    the_case.close
end


RunOCR(utilities, params[:pathArg1], params[:searchTermArg1], params[:ocrProfileArg1], params[:ocrProfilePathArg1])
puts '--Script Completed Successfully--'
