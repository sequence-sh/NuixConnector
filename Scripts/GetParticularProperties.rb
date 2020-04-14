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
	opts.on('--regexArg0 ARG') do |o| params[:regexArg0] = o end
end.parse!

puts params


def GetParticularProperties(utilities,casePathArg,searchArg,regexArg)

    the_case = utilities.case_factory.open(casePathArg)

    puts "Finding Entities"
    items = the_case.search(searchArg, {})
    puts "#{items.length} items found"
    regex = Regexp.new(regexArg)    
    text = "Key\tValue\tPath\tGuid"

    items.each do |i| 
        i.getProperties().each do |k,v|
          text << "#{k}\t#{v}\t#{i.getPathNames().join("/")}\t#{i.getGuid()}" if regex =~ k
        end
    end

    the_case.close
    return text
end



result0 = GetParticularProperties(utilities, params[:casePathArg0], params[:searchArg0], params[:regexArg0])
puts "--Final Result: #{result0}"
