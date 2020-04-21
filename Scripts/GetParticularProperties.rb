#GetParticularProperties

requiredNuixVersion = '6.2'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

require 'optparse'
params = {}
OptionParser.new do |opts|
	opts.on('--casePathArg0 ARG') do |o| params[:casePathArg0] = o end
	opts.on('--searchArg0 ARG') do |o| params[:searchArg0] = o end
	opts.on('--propertyRegexArg0 ARG') do |o| params[:propertyRegexArg0] = o end
	opts.on('--valueRegexArg0 [ARG]') do |o| params[:valueRegexArg0] = o end
end.parse!

puts params


def GetParticularProperties(utilities,casePathArg,searchArg,propertyRegexArg,valueRegexArg)

    the_case = utilities.case_factory.open(casePathArg)

    puts "Finding Entities"
    items = the_case.search(searchArg, {})
    puts "#{items.length} items found"
    propertyRegex = Regexp.new(propertyRegexArg)
    valueRegex = nil
    valueRegex = Regexp.new(valueRegexArg) if valueRegexArg != nil

    text = "Key\tValue\tPath\tGuid"

    items.each do |i| 
        i.getProperties().each do |k,v|
            begin
                if propertyRegex =~ k
                    if valueRegex != nil
                        if match = valueRegex.match(k) #Only output if the value regex actually matches
                            valueString = match.captures[0]
                            text << "\n#{k}\t#{valueString}\t#{i.getPathNames().join("/")}\t#{i.getGuid()}"
                        end
                    else #output the entire value
                        text << "\n#{k}\t#{v}\t#{i.getPathNames().join("/")}\t#{i.getGuid()}"
                    end                                           
                end
            rescue
            end
        end
    end

    the_case.close
    return text
end


def binToHex(s)
  suffix = s.each_byte.map { |b| b.to_s(16).rjust(2, '0') }.join('').upcase
  '0x' + suffix
end



result0 = GetParticularProperties(utilities, params[:casePathArg0], params[:searchArg0], params[:propertyRegexArg0], params[:valueRegexArg0])
puts "--Final Result: #{binToHex(result0)}"
