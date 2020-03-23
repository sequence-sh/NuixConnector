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
  opts.on('--version', 'Display the version') do 
    puts "VERSION"
    exit
  end
  opts.on('-h', '--help', 'Display this help') do 
    puts opts
    exit
  end

end.parse!
requiredArguments = [:pathArg, :productionSetNameArg] 

unless requiredArguments.all? {|a| hash_options[a] != nil}
    puts "Missing arguments #{(requiredArguments.select {|a| hash_options[a] == nil}).to_s}"

else
    
    
    the_case = utilities.case_factory.open(hash_options[:pathArg])

    productionSet = the_case.findProductionSetByName(hash_options[:productionSetNameArg])

        if(productionSet == nil)        
            puts "Production Set Not Found"
        else            
            puts "Production Set Found"

            r = productionSet.getPrintPreviewState()

            puts r
        end 

    the_case.close
    
    
end