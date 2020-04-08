#AddConcordanceToCase

requiredNuixVersion = '7.6'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
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
	opts.on('--pathArg0 ARG') do |o| params[:pathArg0] = o end
	opts.on('--folderNameArg0 ARG') do |o| params[:folderNameArg0] = o end
	opts.on('--folderDescriptionArg0 [ARG]') do |o| params[:folderDescriptionArg0] = o end
	opts.on('--folderCustodianArg0 ARG') do |o| params[:folderCustodianArg0] = o end
	opts.on('--filePathArg0 ARG') do |o| params[:filePathArg0] = o end
	opts.on('--dateFormatArg0 ARG') do |o| params[:dateFormatArg0] = o end
	opts.on('--profileNameArg0 ARG') do |o| params[:profileNameArg0] = o end
end.parse!

puts params


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



AddConcordanceToCase(utilities, params[:pathArg0], params[:folderNameArg0], params[:folderDescriptionArg0], params[:folderCustodianArg0], params[:filePathArg0], params[:dateFormatArg0], params[:profileNameArg0])
puts '--Script Completed Successfully--'
