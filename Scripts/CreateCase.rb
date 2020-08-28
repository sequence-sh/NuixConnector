#CreateCase

requiredNuixVersion = '2.16'
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
	opts.on('--pathArg1 [ARG]') do |o| params[:pathArg1] = o end
	opts.on('--nameArg1 [ARG]') do |o| params[:nameArg1] = o end
	opts.on('--descriptionArg1 [ARG]') do |o| params[:descriptionArg1] = o end
	opts.on('--investigatorArg1 [ARG]') do |o| params[:investigatorArg1] = o end
end.parse!


def CreateCase(utilities,pathArg,nameArg,descriptionArg,investigatorArg)

    puts 'Creating Case'
    the_case = utilities.case_factory.create(pathArg,
    :name => nameArg,
    :description => descriptionArg,
    :investigator => investigatorArg)
    puts 'Case Created'
    the_case.close
end


CreateCase(utilities, params[:pathArg1], params[:nameArg1], params[:descriptionArg1], params[:investigatorArg1])
puts '--Script Completed Successfully--'
