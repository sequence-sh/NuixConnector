require 'optparse'

def bin_to_hex(s)
  suffix = s.each_byte.map { |b| b.to_s(16) }.join('')
  '0x' + suffix
end

hash_options = {}
OptionParser.new do |opts|
  opts.banner = "Usage: your_app [options]"
  opts.on('-p [ARG]', '--path [ARG]', "Case Path") do |v|
    hash_options[:pathArg] = v
  end 
  opts.on('-s [ARG]', '--search [ARG]', "Search Term") do |v|
    hash_options[:searchArg] = v
  end 
  opts.on('-r [ARG]', '--regex [ARG]', "Property Regex") do |v|
    hash_options[:regexArg] = v
  end
  opts.on('-f [ARG]', '--file [ARG]', "Output File Name") do |v|
    hash_options[:fileArg] = v
  end
  opts.on('-h', '--help', 'Display this help') do 
    puts opts
    exit
  end

end.parse!

requiredArguments = [:pathArg, :searchArg, :regexArg, :fileArg]

unless requiredArguments.all? {|a| hash_options[a] != nil}
    puts "Missing arguments #{(requiredArguments.select {|a| hash_options[a] == nil}).to_s}"


else
        
    
    the_case = utilities.case_factory.open(hash_options[:pathArg])

    puts "Finding Entities"

    items = the_case.search(hash_options[:searchArg], {})

    puts "#{items.length} items found"

    regex = Regexp.new(hash_options[:regexArg])
    
    puts "Output#{hash_options[:fileArg]}:Key\tValue\tPath\tGuid"


    items.each do |i| 

        i.getProperties().each do |k,v|

          puts "Output#{hash_options[:fileArg]}:#{k}\t#{v}\t#{i.getPathNames().join("/")}\t#{i.getGuid()}" if regex =~ k

        end

    end
   
    the_case.close
        
end