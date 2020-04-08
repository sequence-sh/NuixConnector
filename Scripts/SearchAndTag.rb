require 'optparse'
#SearchAndTag
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG') do |o| params[:pathArg0] = o end
opts.on('--searchArg0 ARG') do |o| params[:searchArg0] = o end
opts.on('--tagArg0 ARG') do |o| params[:tagArg0] = o end
end.parse!
puts params

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

    puts "#{j} items tagged"
    the_case.close
end



SearchAndTag(utilities, params[:pathArg0], params[:searchArg0], params[:tagArg0])
puts '--Script Completed Successfully--'
