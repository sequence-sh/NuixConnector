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
	opts.on('--pathArg1 [ARG]') do |o| params[:pathArg1] = o end
	opts.on('--folderNameArg1 [ARG]') do |o| params[:folderNameArg1] = o end
	opts.on('--folderDescriptionArg1 [ARG]') do |o| params[:folderDescriptionArg1] = o end
	opts.on('--folderCustodianArg1 [ARG]') do |o| params[:folderCustodianArg1] = o end
	opts.on('--filePathArg1 [ARG]') do |o| params[:filePathArg1] = o end
	opts.on('--processingProfileNameArg1 [ARG]') do |o| params[:processingProfileNameArg1] = o end
	opts.on('--processingProfilePathArg1 [ARG]') do |o| params[:processingProfilePathArg1] = o end
	opts.on('--passwordFilePathArg1 [ARG]') do |o| params[:passwordFilePathArg1] = o end
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


AddToCase(utilities, params[:pathArg1], params[:folderNameArg1], params[:folderDescriptionArg1], params[:folderCustodianArg1], params[:filePathArg1], params[:processingProfileNameArg1], params[:processingProfilePathArg1], params[:passwordFilePathArg1])
puts '--Script Completed Successfully--'
