#CountItems

requiredNuixVersion = '5.0'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

require 'optparse'
params = {}
OptionParser.new do |opts|
	opts.on('--pathArg1a [ARG]') do |o| params[:pathArg1a] = o end
	opts.on('--searchArg1a [ARG]') do |o| params[:searchArg1a] = o end
end.parse!


def CountItems(utilities,pathArg,searchArg)

    the_case = utilities.case_factory.open(pathArg)
    searchOptions = {}
    count = the_case.count(searchArg, searchOptions)
    the_case.close
    puts "#{count} found matching '#{searchArg}'"
    return count
end

def BinToHex(s)
suffix = s.to_s.each_byte.map { |b| b.to_s(16).rjust(2, '0') }.join('').upcase
'0x' + suffix
end


BinToHexBinToHex1 = (CountItems1a)
puts "--Final Result: #{binToHex(BinToHex1)}"
