﻿require 'optparse'
#CountItems
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG') do |o| params[:pathArg0] = o end
opts.on('--searchArg0 ARG') do |o| params[:searchArg0] = o end
end.parse!
puts params

def CountItems(utilities,pathArg,searchArg)

    the_case = utilities.case_factory.open(pathArg)
    searchOptions = {}
    count = the_case.count(searchArg, searchOptions)
    the_case.close  
    puts "#{count} found matching '#{searchArg}'"
    return count
    
end



result0 = CountItems(utilities, params[:pathArg0], params[:searchArg0])
puts "--Final Result: #{result0}"
