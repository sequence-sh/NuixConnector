#DoesCaseExist

requiredNuixVersion = '5.0'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

require 'optparse'
params = {}
OptionParser.new do |opts|
	opts.on('--pathArg0 ARG') do |o| params[:pathArg0] = o end
end.parse!


def DoesCaseExist(utilities,pathArg)

    begin
        the_case = utilities.case_factory.open(pathArg)
        the_case.close()
        return true
    rescue #Case does not exist
        return false
    end

end


def binToHex(s)
  suffix = s.to_s.each_byte.map { |b| b.to_s(16).rjust(2, '0') }.join('').upcase
  '0x' + suffix
end



result0 = DoesCaseExist(utilities, params[:pathArg0])
puts "--Final Result: #{binToHex(result0)}"
