require 'optparse'

hash_options = {}
OptionParser.new do |opts|
  opts.banner = "Usage: your_app [options]"
  opts.on('-p [ARG]', '--path [ARG]', "Case Path") do |v|
    hash_options[:pathArg] = v
  end
  opts.on('-s [ARG]', '--searchTerm [ARG]', "Search Term") do |v|
    hash_options[:searchArg] = v
  end
  opts.on('-n [ARG]', '--productionSetName [ARG]', "Production Set Name") do |v|
    hash_options[:productionSetNameArg] = v
  end
  opts.on('-o [ARG]', '--orderTerm [ARG]', "Order Term") do |v|
    hash_options[:orderArg] = v
  end
  opts.on('-l [ARG]', '--limit [ARG]', "Limit") do |v|
    hash_options[:limitArg] = v
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

requiredArguments = [:pathArg, :productionSetNameArg, :searchArg] #orderArg and limitArg are optional

unless requiredArguments.all? {|a| hash_options[a] != nil}
    puts "Missing arguments #{(requiredArguments.select {|a| hash_options[a] == nil}).to_s}"


else
    puts "Opening Case"
    
    the_case = utilities.case_factory.open(hash_options[:pathArg])

    productionSet = findProductionSetByName(hash_options[:productionSetNameArg])

    if(productionSet == nil){
        productionSet = the_case.newProductionSet(hash_options[:productionSetNameArg])
        
        puts "Production Set Created"
    }else{
        puts "Production Set Found"
    }
    end    

    puts "Searching"

    searchOptions = {}
    searchOptions[:order] = hash_options[:orderArg] if hash_options[:orderArg] != nil
    searchOptions[:limit] = hash_options[:limitArg].to_i if hash_options[:limitArg] != nil


    items = the_case.search(hash_options[:searchArg], searchOptions)

    puts "#{items.length} found"

    productionSet.addItems(items)

    puts "items added"

    the_case.close
    puts "Case Closed"
    
end