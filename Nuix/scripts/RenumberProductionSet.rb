require 'optparse'

hash_options = {}
OptionParser.new do |opts|
  opts.banner = "Usage: your_app [options]"
  opts.on('-p [ARG]', '--path [ARG]', "Case Path") do |v|
    hash_options[:pathArg] = v
  end
  opts.on('-n [ARG]', '--productionSetName [ARG]', "Production Set Name") do |v|
    hash_options[:productionSetNameArg] = v
  end
  opts.on('-s [ARG]', '--sortOrder [ARG]', "Sort Order") do |v|
    hash_options[:sortOrderArg] = v
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
requiredArguments = [:pathArg, :productionSetNameArg, :sortOrderArg] 

unless requiredArguments.all? {|a| hash_options[a] != nil}
    puts "Missing arguments #{(requiredArguments.select {|a| hash_options[a] == nil}).to_s}"


else
    puts "Opening Case"
    
    the_case = utilities.case_factory.open(hash_options[:pathArg])

    productionSet = the_case.findProductionSetByName(hash_options[:productionSetNameArg])

        if(productionSet == nil)        
            puts "Production Set Not Found"
        else            
            puts "Production Set Found"

            options = 
            {
                sortOrder: hash_options[:sortOrderArg]
            }

            resultMap = productionSet.renumber(options)

            puts resultMap

        end 

    the_case.close
    puts "Case Closed"
    
end