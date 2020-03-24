def GetParticularProperties(pathArg, searchArg, regexArg, fileArg)

        
    
    the_case = utilities.case_factory.open(pathArg)

    puts "Finding Entities"

    items = the_case.search(searchArg, {})

    puts "#{items.length} items found"

    regex = Regexp.new(regexArg)
    
    puts "Output#{fileArg}:Key\tValue\tPath\tGuid"


    items.each do |i| 

        i.getProperties().each do |k,v|

          puts "Output#{fileArg}:#{k}\t#{v}\t#{i.getPathNames().join("/")}\t#{i.getGuid()}" if regex =~ k

        end

    end
   
    the_case.close
        
end