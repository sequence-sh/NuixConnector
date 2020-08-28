#MigrateCase

requiredNuixVersion = '3.0'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

require 'optparse'
params = {}
OptionParser.new do |opts|
	opts.on('--pathArg1 [ARG]') do |o| params[:pathArg1] = o end
end.parse!


def MigrateCase(utilities,pathArg)

    puts "Opening Case, migrating if necessary"

    options = {migrate: true}

    the_case = utilities.case_factory.open(pathArg, options)
end


MigrateCase(utilities, params[:pathArg1])
puts '--Script Completed Successfully--'
