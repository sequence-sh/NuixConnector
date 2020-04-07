require 'optparse'
#AddToCase
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG')
opts.on('--folderNameArg0 ARG')
opts.on('--folderDescriptionArg0 [ARG]')
opts.on('--folderCustodianArg0 ARG')
opts.on('--filePathArg0 ARG')
opts.on('--processingProfileNameArg0 [ARG]')
end.parse!(into: params)
puts params

def AddToCase(utilities,pathArg,folderNameArg,folderDescriptionArg,folderCustodianArg,filePathArg,processingProfileNameArg)

    the_case = utilities.case_factory.open(pathArg)
    processor = the_case.create_processor
    processor.setProcessingProfile(processingProfileNameArg) if processingProfileNameArg != nil

    folder = processor.new_evidence_container(folderNameArg)

    folder.description = folderDescriptionArg if folderDescriptionArg != nil
    folder.initial_custodian = folderCustodianArg

    folder.add_file(filePathArg)
    folder.save

    puts 'Starting processing.'
    processor.process
    puts 'Processing complete.'
    the_case.close
end



AddToCase(utilities, params[:pathArg0], params[:folderNameArg0], params[:folderDescriptionArg0], params[:folderCustodianArg0], params[:filePathArg0], params[:processingProfileNameArg0])
puts '--Script Completed Successfully--'
