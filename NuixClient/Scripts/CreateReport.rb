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

    termStatistics.each do |term, count|
        puts "OutputTerms:#{term}: #{count}"
    end
    
    allItems = the_case.searchUnsorted("")

    results = Hash.new(Hash.new(Hash.new(0)))

    items.each do |i|
        custodians = ["any"]
        custodians << i.getCustodian() if i.getCustodian() != nil

        custodians.each do |c|
            hash = results[c]

            kindsHash = results[:kinds]
            kindsHash["*"] += 1
            kindsHash[i.getKind().getName()]  += 1

            typesHash = results[:types]            
            typesHash[i.getType().getName()] += 1            

            communication = i.getCommunication()
            if communication != nil
                addresses = i.getFrom() | i.getTo() | i.getCc() | i.getBCc()
                addressesHash = results[:addresses]
                addresses.each do |a|
                    addressesHash[a] += 1
                end
            end

            tagsHash = results[:tags]
            i.getTags().each do |t|
                tagsHash[t] += 1
            end

            language = i.getLanguage()
            if language != nil
                languageHash = results[:language]
                languageHash[language] += 1
            end
        end
    end

    results.each do |custodian, hash1|
        hash1.each do |type, hash2|
            hash2.each do |value, count|
                puts "OutputStats:#{custodian}\t#{type}\t#{value}\t#{count}"
            end
        end
    end

    the_case.close
    puts "Case Closed"
    
end