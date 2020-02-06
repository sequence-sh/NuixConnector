require 'optparse'

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
    hash_options[:descriptionArg] = v
  end
  opts.on('-c [ARG]', '--folderCustodianArg [ARG]', "Specify the Custodian") do |v|
    hash_options[:folderCustodianArg] = v
  end
  opts.on('-f [ARG]', '--filePath [ARG]', "Specify the File to add path") do |v|
    hash_options[:filePathArg] = v
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

requiredArguments = [:pathArg, :folderNameArg, :folderDescriptionArg, :folderCustodianArg :filePathArg]

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
    #TODO other options
    #folder.encoding = 'UTF-8'
    #folder.timeZone = 'Pacific/Honolulu'
    #folder.custom_metadata = { 'Barcode' => '1234-567-890' }
    folder.add_file(hash_options[:filePathArg])
    folder.save

    puts 'Starting processing.'
    processor.process
    puts 'Processing complete.'
        the_case.close
    puts "Case Closed"
    
end