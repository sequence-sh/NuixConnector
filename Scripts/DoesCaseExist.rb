require 'optparse'
#DoesCaseExist
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG')
end.parse!(into: params)
puts params

def DoesCaseExist(utilities,pathArg)

    begin
        the_case = utilities.case_factory.open(pathArg)
        the_case.close()
        return true
    rescue #Case does not exist
        return false
    end

end



result0 = DoesCaseExist(utilities, params[:pathArg0])
puts "--Final Result: #{result0}"
