require 'optparse'

hash_options = {}
OptionParser.new do |opts|
  opts.banner = "Usage: your_app [options]"
  opts.on('-p [ARG]', '--path [ARG]', "Case Path") do |v|
    hash_options[:pathArg] = v
  end
  opts.on('-x [ARG]', '--exportPath [ARG]', "Export Path") do |v|
    hash_options[:exportPathArg] = v
  end
  opts.on('-n [ARG]', '--productionSetName [ARG]', "Production set name") do |v|
    hash_options[:productionSetNameArg] = v
  end
  opts.on('-m [ARG]', '--metadataProfile [ARG]', "Metadata Profile Name") do |v| #this is actually optional
    hash_options[:metadataProfileArg] = v 
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

requiredArguments = [:pathArg, :exportPathArg, :productionSetNameArg]

unless requiredArguments.all? {|a| hash_options[a] != nil}
    puts "Missing arguments #{(requiredArguments.select {|a| hash_options[a] == nil}).to_s}"


else
    puts "Opening Case"
    
    the_case = utilities.case_factory.open(hash_options[:pathArg])

    productionSet = the_case.findProductionSetByName(hash_options[:productionSetNameArg])

    if productionSet == nil

        puts "Could not find production set with name '#{:productionSetNameArg.to_s}'"

    else
        batchExporter = utilities.createBatchExporter(hash_options[:exportPathArg])

        batchExporter.addLoadFile("concordance",{
        metadataProfile => hash_options[:metadataProfileArg]
		})


        puts 'Starting export.'
        batchExporter.exportItems(productionSet)        
        puts 'Export complete.'

    end

    the_case.close
    puts "Case Closed"
    
end