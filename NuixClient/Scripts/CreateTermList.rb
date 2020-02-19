require 'optparse'

hash_options = {}
OptionParser.new do |opts|
  opts.banner = "Usage: your_app [options]"
  opts.on('-p [ARG]', '--path [ARG]', "Case Path") do |v|
    hash_options[:pathArg] = v
  end 
  opts.on('-h', '--help', 'Display this help') do 
    puts opts
    exit
  end

end.parse!

requiredArguments = [:pathArg]

unless requiredArguments.all? {|a| hash_options[a] != nil}
    puts "Missing arguments #{(requiredArguments.select {|a| hash_options[a] == nil}).to_s}"


else
    puts "Opening Case"
    
    the_case = utilities.case_factory.open(hash_options[:pathArg])

    puts "Generating Report:"   

    caseStatistics = the_case.getStatistics()
    dateRange = caseStatistics.getCaseDateRange()
    
    puts "Date Range:#{dateRange.getEarliest()} to #{dateRange.getLatest()}"

    termStatistics = caseStatistics.getTermStatistics("", {"sort" => "on", "deduplicate" => "md5"}) #for some reason this takes strings rather than symbols
    #todo terms per custodian
    puts "#{termStatistics.length} terms"

    puts "OutputTerms:term\tcount"

    termStatistics.each do |term, count|
        puts "OutputTerms:#{term}\t#{count}"
    end
   
    the_case.close
    puts "Case Closed"    
end