def CreateTermList(pathArg)

        
    
    the_case = utilities.case_factory.open(pathArg)

    puts "Generating Report:"   

    caseStatistics = the_case.getStatistics()

    termStatistics = caseStatistics.getTermStatistics("", {"sort" => "on", "deduplicate" => "md5"}) #for some reason this takes strings rather than symbols
    #todo terms per custodian
    puts "#{termStatistics.length} terms"

    puts "OutputTerms:Term\tCount"

    termStatistics.each do |term, count|
        puts "OutputTerms:#{bin_to_hex(term)}\t#{count}"
    end
   
    the_case.close
        
end