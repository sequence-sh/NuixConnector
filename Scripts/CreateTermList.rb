#BinToHex

requiredNuixVersion = '5.0'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	raise "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

require 'optparse'
params = {}
OptionParser.new do |opts|
	opts.on('--casePathArg1a [ARG]') do |o| params[:casePathArg1a] = o end
end.parse!


def CreateTermList(utilities,casePathArg)

    the_case = utilities.case_factory.open(casePathArg)

    puts "Generating Report:"
    caseStatistics = the_case.getStatistics()
    termStatistics = caseStatistics.getTermStatistics("", {"sort" => "on", "deduplicate" => "md5"}) #for some reason this takes strings rather than symbols
    #todo terms per custodian
    puts "#{termStatistics.length} terms"

    text = "Term\tCount"

    termStatistics.each do |term, count|
        text << "\n#{term}\t#{count}"
    end

    the_case.close
    return text
end

def BinToHex(s)
suffix = s.to_s.each_byte.map { |b| b.to_s(16).rjust(2, '0') }.join('').upcase
'0x' + suffix
end


CreateTermList1a = CreateTermList(utilities, params[:casePathArg1a])
bintohex1 = BinToHex(CreateTermList1a)
bintohex1
puts "--Final Result: #{bintohex1}"
