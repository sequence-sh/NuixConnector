#AddToCase

requiredNuixVersion = '3.2'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

requiredFeatures = Array['CASE_CREATION']
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
	opts.on('--processingProfileNameArg0 [ARG]') do |o| params[:processingProfileNameArg0] = o end
	opts.on('--processingProfilePathArg0 [ARG]') do |o| params[:processingProfilePathArg0] = o end
	opts.on('--passwordFilePathArg0 [ARG]') do |o| params[:passwordFilePathArg0] = o end
end.parse!


def AddToCase(utilities,pathArg,folderNameArg,folderDescriptionArg,folderCustodianArg,filePathArg,processingProfileNameArg,processingProfilePathArg,passwordFilePathArg)

    the_case = utilities.case_factory.open(pathArg)
    processor = the_case.create_processor

#This only works in 7.6 or later
    if processingProfileNameArg != nil
        processor.setProcessingProfile(processingProfileNameArg) 
    elsif processingProfilePathArg != nil
        profileBuilder = utilities.getProcessingProfileBuilder()
        profileBuilder.load(processingProfilePathArg)
        profile = profileBuilder.build()

        if profile == nil
            puts "Could not find processing profile at #{processingProfilePathArg}"
            exit
        end

        processor.setProcessingProfileObject(profile)
    end


#This only works in 7.2 or later
    if passwordFilePathArg != nil
        lines = File.read(passwordFilePathArg, mode: 'r:bom|utf-8').split

        passwords = lines.map {|p| p.chars.to_java(:char)}
        listName = 'MyPasswordList'

        processor.addPasswordList(listName, passwords)
        processor.setPasswordDiscoverySettings({'mode' => "word-list", 'word-list' => listName })
    end


    folder = processor.new_evidence_container(folderNameArg)

    folder.description = folderDescriptionArg if folderDescriptionArg != nil
    folder.initial_custodian = folderCustodianArg

    folder.add_file(filePathArg)
    folder.save

    puts 'Adding items'
    processor.process
    puts 'Items added'
    the_case.close
end



AddToCase(utilities, params[:pathArg0], params[:folderNameArg0], params[:folderDescriptionArg0], params[:folderCustodianArg0], params[:filePathArg0], params[:processingProfileNameArg0], params[:processingProfilePathArg0], params[:passwordFilePathArg0])
puts '--Script Completed Successfully--'
