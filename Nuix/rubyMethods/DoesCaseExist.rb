def DoesCaseExist(pathArg)

    

    begin
        the_case = utilities.case_factory.open(pathArg)
        puts "Case Exists"
        the_case.close
    rescue
         puts "Case does not exist"
    end
    
   
end