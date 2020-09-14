#AddConcordanceToCase

requiredNuixVersion = '7.6'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	raise "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

requiredFeatures = Array['CASE_CREATION', 'METADATA_IMPORT']
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
	opts.on('--folderNameArg1 [ARG]') do |o| params[:folderNameArg1] = o end
	opts.on('--folderDescriptionArg1 [ARG]') do |o| params[:folderDescriptionArg1] = o end
	opts.on('--folderCustodianArg1 [ARG]') do |o| params[:folderCustodianArg1] = o end
	opts.on('--filePathArg1 [ARG]') do |o| params[:filePathArg1] = o end
	opts.on('--dateFormatArg1 [ARG]') do |o| params[:dateFormatArg1] = o end
	opts.on('--profileNameArg1 [ARG]') do |o| params[:profileNameArg1] = o end
end.parse!


def AddConcordanceToCase(utilities,pathArg,folderNameArg,folderDescriptionArg,folderCustodianArg,filePathArg,dateFormatArg,profileNameArg)

    the_case = utilities.case_factory.open(pathArg)
    processor = the_case.create_processor
    processor.processing_settings = { :create_thumbnails       => false,
                                    :additional_digests      => [ 'SHA-1' ] }


    folder = processor.new_evidence_container(folderNameArg)

    folder.description = folderDescriptionArg
    folder.initial_custodian = folderCustodianArg
    folder.addLoadFile({
    :concordanceFile => filePathArg,
    :concordanceDateFormat => dateFormatArg
    })
    folder.setMetadataImportProfileName(profileNameArg)
    folder.save

    puts 'Starting processing.'
    processor.process
    puts 'Processing complete.'
    the_case.close
end


AddConcordanceToCase(utilities, params[:pathArg1], params[:folderNameArg1], params[:folderDescriptionArg1], params[:folderCustodianArg1], params[:filePathArg1], params[:dateFormatArg1], params[:profileNameArg1])
puts '--Script Completed Successfully--'
