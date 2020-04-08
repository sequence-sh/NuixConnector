#CreateCase

requiredNuixVersion = '5.0'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

requiredFeatures = Array['CASE_CREATION']
requiredFeatures.each do |feature|
	if !utilities.getLicence().hasFeature(feature)
		puts "Nuix Feature #{feature} is required but not available."
		exit
	end
end

require 'optparse'
params = {}
OptionParser.new do |opts|
	opts.on('--pathArg0 ARG') do |o| params[:pathArg0] = o end
	opts.on('--nameArg0 ARG') do |o| params[:nameArg0] = o end
	opts.on('--descriptionArg0 [ARG]') do |o| params[:descriptionArg0] = o end
	opts.on('--investigatorArg0 ARG') do |o| params[:investigatorArg0] = o end
end.parse!

puts params


def CreateCase(utilities,pathArg,nameArg,descriptionArg,investigatorArg)

    puts 'Creating Case'    
    the_case = utilities.case_factory.create(pathArg,
    :name => nameArg,
    :description => descriptionArg,
    :investigator => investigatorArg)
    puts 'Case Created'
    the_case.close
end



CreateCase(utilities, params[:pathArg0], params[:nameArg0], params[:descriptionArg0], params[:investigatorArg0])
puts '--Script Completed Successfully--'
