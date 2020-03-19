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
  opts.on('-t [ARG]', '--tagTerm [ARG]', "Tag") do |v|
    hash_options[:tagArg] = v
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

requiredArguments = [:pathArg, :tagArg, :searchArg] #orderArg and limitArg are optional

unless requiredArguments.all? {|a| hash_options[a] != nil}
    puts "Missing arguments #{(requiredArguments.select {|a| hash_options[a] == nil}).to_s}"


else
    
    
    the_case = utilities.case_factory.open(hash_options[:pathArg])

    puts "Searching for '#{hash_options[:searchArg]}'"

    searchOptions = {}
    searchOptions[:order] = hash_options[:orderArg] if hash_options[:orderArg] != nil
    searchOptions[:limit] = hash_options[:limitArg].to_i if hash_options[:limitArg] != nil


    items = the_case.search(hash_options[:searchArg], searchOptions)

    puts "#{items.length} found"

    j = 0

    items.each {|i|
       added = i.addTag(hash_options[:tagArg])
       j += 1 if added
    }

    puts "#{j} items tagged"

    the_case.close
    
    
end