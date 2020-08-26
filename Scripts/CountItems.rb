#CountItems

requiredNuixVersion = '5.0'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

require 'optparse'
params = {}
OptionParser.new do |opts|
	opts.on('--pathArg0 ARG') do |o| params[:pathArg0] = o end
	opts.on('--searchArg0 ARG') do |o| params[:searchArg0] = o end
end.parse!

puts params


def CountItems(utilities,pathArg,searchArg)

    the_case = utilities.case_factory.open(pathArg)
    searchOptions = {}
    count = the_case.count(searchArg, searchOptions)
    the_case.close
    puts "#{count} found matching '#{searchArg}'"
    return count

end


def binToHex(s)
  suffix = s.each_byte.map { |b| b.to_s(16).rjust(2, '0') }.join('').upcase
  '0x' + suffix
end



result0 = CountItems(utilities, params[:pathArg0], params[:searchArg0])
puts "--Final Result: #{binToHex(result0)}"
