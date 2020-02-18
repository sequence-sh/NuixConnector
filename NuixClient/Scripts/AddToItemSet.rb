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
  opts.on('-n [ARG]', '--itemSetName [ARG]', "Item Set Name") do |v|
    hash_options[:productionSetNameArg] = v
  end
  opts.on('-o [ARG]', '--orderTerm [ARG]', "Order Term") do |v|
    hash_options[:orderArg] = v
  end
  opts.on('-l [ARG]', '--limit [ARG]', "Limit") do |v|
    hash_options[:limitArg] = v
   end
  opts.on('-d [ARG]', '--deduplication [ARG]', "Deduplication") do |v|
    hash_options[:deduplicationArg] = v
  end
  opts.on('-r [ARG]', '--description [ARG]', "Item Set Description") do |v|
    hash_options[:descriptionArg] = v
  end
  opts.on('-b [ARG]', '--deduplicateBy [ARG]', "Deduplicate By") do |v|
    hash_options[:deduplicateByArg] = v
  end
  opts.on('-c [ARG]', '--deduplicateBy [ARG]', "Custodian Ranking (Comma Separated)") do |v|
    hash_options[:custodianRankingArg] = v
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

requiredArguments = [:pathArg, :productionSetNameArg, :searchArg] #deduplicateByArg descriptionArg, deduplicationArg, orderArg and limitArg are optional

unless requiredArguments.all? {|a| hash_options[a] != nil}
    puts "Missing arguments #{(requiredArguments.select {|a| hash_options[a] == nil}).to_s}"


else
    puts "Opening Case"
    
    the_case = utilities.case_factory.open(hash_options[:pathArg])

    itemSet = the_case.findItemSetByName(hash_options[:productionSetNameArg])

    if(itemSet == nil)
        itemSetOptions = {}

        itemSetOptions[:deduplication] = hash_options[:deduplicationArg] if(hash_options[:deduplicationArg] != nil)
        itemSetOptions[:description] = hash_options[:descriptionArg] if(hash_options[:descriptionArg] != nil)
        itemSetOptions[:deduplicateBy] = hash_options[:deduplicateByArg] if(hash_options[:deduplicateByArg] != nil)
        itemSetOptions[:custodianRanking] = hash_options[:custodianRankingArg].split(",") if(hash_options[:custodianRankingArg] != nil)

        itemSet = the_case.createItemSet(hash_options[:productionSetNameArg], itemSetOptions)
        
        puts "Item Set Created"
    else
        puts "Item Set Found"
    end    

    puts "Searching"

    searchOptions = {}
    searchOptions[:order] = hash_options[:orderArg] if hash_options[:orderArg] != nil
    searchOptions[:limit] = hash_options[:limitArg].to_i if hash_options[:limitArg] != nil

    items = the_case.search(hash_options[:searchArg], searchOptions)

    puts "#{items.length} found"

    itemSet.addItems(items)

    puts "items added"

    the_case.close
    puts "Case Closed"
    
end