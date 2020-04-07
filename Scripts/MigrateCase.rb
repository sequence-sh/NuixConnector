require 'optparse'
#MigrateCase
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG')
end.parse!(into: params)
puts params

def MigrateCase(utilities,pathArg)

    puts "Opening Case, migrating if necessary"
    
    options = {migrate: true}

    the_case = utilities.case_factory.open(pathArg, options)
end



MigrateCase(utilities, params[:pathArg0])
puts '--Script Completed Successfully--'
