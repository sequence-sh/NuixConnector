require 'optparse'
#CreateTermList
params = {}
OptionParser.new do |opts|
opts.on('--casePathArg0 ARG') do |o| params[:casePathArg0] = o end
opts.on('--outputFilePathArg0 ARG') do |o| params[:outputFilePathArg0] = o end
end.parse!
puts params

def CreateTermList(utilities,casePathArg,outputFilePathArg)

    the_case = utilities.case_factory.open(casePathArg)

    puts "Generating Report:"   
    caseStatistics = the_case.getStatistics()
    termStatistics = caseStatistics.getTermStatistics("", {"sort" => "on", "deduplicate" => "md5"}) #for some reason this takes strings rather than symbols
    #todo terms per custodian
    puts "#{termStatistics.length} terms"

    text = "Terms:Term\tCount"

    termStatistics.each do |term, count|
        text << "\n#{term}\t#{count}"
    end

    File.write(outputFilePathArg, text)
   
    the_case.close
end



CreateTermList(utilities, params[:casePathArg0], params[:outputFilePathArg0])
puts '--Script Completed Successfully--'
