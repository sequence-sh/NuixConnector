#SearchAndTag

requiredNuixVersion = '2.16'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

requiredFeatures = Array['ANALYSIS']
requiredFeatures.each do |feature|
	if !utilities.getLicence().hasFeature(feature)
		puts "Nuix Feature #{feature} is required but not available."
		exit
	end
end

require 'optparse'
params = {}
OptionParser.new do |opts|
	opts.on('--pathArg1 [ARG]') do |o| params[:pathArg1] = o end
	opts.on('--searchArg1 [ARG]') do |o| params[:searchArg1] = o end
	opts.on('--tagArg1 [ARG]') do |o| params[:tagArg1] = o end
end.parse!


def SearchAndTag(utilities,pathArg,searchArg,tagArg)

    the_case = utilities.case_factory.open(pathArg)
    puts "Searching for '#{searchArg}'"

    searchOptions = {}
    items = the_case.search(searchArg, searchOptions)
    puts "#{items.length} found"

    j = 0

    items.each {|i|
       added = i.addTag(tagArg)
       j += 1 if added
    }

    puts "#{j} items tagged with #{tagArg}"
    the_case.close
end


SearchAndTag(utilities, params[:pathArg1], params[:searchArg1], params[:tagArg1])
puts '--Script Completed Successfully--'
