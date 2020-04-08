require 'optparse'
#CreateCase
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
