#GetPrintPreviewState

requiredNuixVersion = '5.2'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	raise "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

requiredFeatures = Array['ANALYSIS', 'PRODUCTION_SET']
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
	opts.on('--productionSetNameArg1 [ARG]') do |o| params[:productionSetNameArg1] = o end
	opts.on('--expectedStateArg1 [ARG]') do |o| params[:expectedStateArg1] = o end
end.parse!


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


GetPrintPreviewState(utilities, params[:pathArg1], params[:productionSetNameArg1], params[:expectedStateArg1])
puts '--Script Completed Successfully--'
