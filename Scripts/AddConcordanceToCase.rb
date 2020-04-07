﻿require 'optparse'
#AddConcordanceToCase
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG')
opts.on('--folderNameArg0 ARG')
opts.on('--folderDescriptionArg0 [ARG]')
opts.on('--folderCustodianArg0 ARG')
opts.on('--filePathArg0 ARG')
opts.on('--dateFormatArg0 ARG')
opts.on('--profileNameArg0 ARG')
end.parse!(into: params)
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