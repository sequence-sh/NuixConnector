require 'optparse'

hash_options = {}
OptionParser.new do |opts|
  opts.banner = "Usage: your_app [options]"
  opts.on('-p [ARG]', '--path [ARG]', "Path") do |v|
    hash_options[:pathArg] = v
  end
  opts.on('-n [ARG]', '--name [ARG]', "Specify the Name") do |v|
    hash_options[:nameArg] = v
  end
  opts.on('-d [ARG]', '--description [ARG]', "Specify the Description") do |v|
    hash_options[:descriptionArg] = v
  end
  opts.on('-i [ARG]', '--investigator [ARG]', "Specify the Investigator") do |v|
    hash_options[:investigatorArg] = v
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

requiredArguments = [:pathArg, :nameArg, :descriptionArg, :investigatorArg]

unless requiredArguments.all? {|a| hash_options[a] != nil}
    puts "Missing arguments #{(requiredArguments.select {|a| hash_options[a] == nil}).to_s}"


else
    puts "Creating Case"
    
        the_case = utilities.case_factory.create(hash_options[:pathArg],
            :name => hash_options[:nameArg],
            :description => hash_options[:descriptionArg],
            :investigator => hash_options[:investigatorArg])
    puts "Case Created"
        the_case.close
    puts "Case Closed"
    
end




