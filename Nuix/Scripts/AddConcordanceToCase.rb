require 'optparse'

#Adds a concordance to a case

hash_options = {}
OptionParser.new do |opts|
  opts.banner = "Usage: your_app [options]"
  opts.on('-p [ARG]', '--path [ARG]', "Path") do |v|
    hash_options[:pathArg] = v
  end
  opts.on('-n [ARG]', '--folderName [ARG]', "Specify the Folder Name") do |v|
    hash_options[:folderNameArg] = v
  end
  opts.on('-d [ARG]', '--folderDescriptionArg [ARG]', "Specify the Folder Description") do |v|
    hash_options[:folderDescriptionArg] = v
  end
  opts.on('-c [ARG]', '--folderCustodianArg [ARG]', "Specify the Custodian") do |v|
    hash_options[:folderCustodianArg] = v
  end
  opts.on('-f [ARG]', '--filePath [ARG]', "Specify the concordance file path") do |v|
    hash_options[:filePathArg] = v
  end
  opts.on('-z [ARG]', '--dateformat [ARG]', "Specify the concordance date format") do |v|
    hash_options[:dateFormatArg] = v
  end
  opts.on('-t [ARG]', '--profileName [ARG]', "Specify the concordance profileName") do |v|
    hash_options[:profileNameArg] = v
  end

  opts.on('--version', 'Display the version') do 
    puts "VERSION"
    exit
  end
  opts.on('-h', '--help', 'Display this help') do 
    puts opts
    exit
  end
end.parse!

requiredArguments = [:pathArg, :folderNameArg, :folderDescriptionArg, :folderCustodianArg, :filePathArg, :dateFormatArg, :profileNameArg]

unless requiredArguments.all? {|a| hash_options[a] != nil}
    puts "Missing arguments #{(requiredArguments.select {|a| hash_options[a] == nil}).to_s}"


else
    puts "Opening Case"
    
    the_case = utilities.case_factory.open(hash_options[:pathArg])

    processor = the_case.create_processor
    processor.processing_settings = { :create_thumbnails       => false,
                                    :additional_digests      => [ 'SHA-1' ] }


    folder = processor.new_evidence_container(hash_options[:folderNameArg])

    folder.description = hash_options[:folderDescriptionArg]
    folder.initial_custodian = hash_options[:folderCustodianArg]
    folder.addLoadFile({
    :concordanceFile => hash_options[:filePathArg],
    :concordanceDateFormat => hash_options[:dateFormatArg]
    })
    folder.setMetadataImportProfileName(hash_options[:profileNameArg])
    folder.save

    puts 'Starting processing.'
    processor.process
    puts 'Processing complete.'
    the_case.close
    puts "Case Closed"
    
end