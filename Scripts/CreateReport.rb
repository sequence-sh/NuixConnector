﻿require 'optparse'
#CreateReport
params = {}
OptionParser.new do |opts|
opts.on('--casePathArg0 ARG')
opts.on('--outputFilePathArg0 ARG')
end.parse!(into: params)
puts params

def CreateReport(utilities,casePathArg,outputFilePathArg)

    the_case = utilities.case_factory.open(casePathArg)

    puts "Generating Report:"
    allItems = the_case.searchUnsorted("")
    results = Hash.new { |h, k| h[k] = Hash.new { |hh, kk| hh[kk] = Hash.new{0} } }

    allItems.each do |i|
        custodians = ["*"]
        custodians << i.getCustodian() if i.getCustodian() != nil

        custodians.each do |c|
            hash = results[c]            

            kindsHash = hash[:kind]
            kindsHash["*"] += 1
            kindsHash[i.getKind().getName()]  += 1

            typesHash = hash[:type]            
            typesHash[i.getType().getName()] += 1

            tagsHash = hash[:tag]
            i.getTags().each do |t|
                tagsHash[t] += 1
            end

            language = i.getLanguage()
            if language != nil
                languageHash = hash[:language]
                languageHash[language] += 1
            end

            communication = i.getCommunication()
            if communication != nil
                
                from = communication.getFrom()
                to = communication.getTo()
                cc = communication.getCc()
                bcc = communication.getBcc()

                addressesHash = hash[:address]
                from.each { |a|  addressesHash[a] += 1} if from != nil
                to.each { |a|  addressesHash[a] += 1} if to != nil
                cc.each { |a|  addressesHash[a] += 1} if cc != nil
                bcc.each { |a|  addressesHash[a] += 1} if bcc != nil
            end            
        end
    end

    puts "Created results for #{allItems.length} items"

    text = "Custodian\tType\tValue\tCount"

    puts "#{results.length - 1} custodians"
    results.each do |custodian, hash1|
        hash1.each do |type, hash2|
            puts "#{custodian} has #{hash2.length} #{type}s" if custodian != "*"
            hash2.sort_by{|value, count| -count}.each do |value, count|
                text <<  "#{custodian}\t#{type}\t#{value}\t#{count}"
            end
        end
    end
    
    File.write(outputFilePathArg, text)

    the_case.close
end



CreateReport(utilities, params[:casePathArg0], params[:outputFilePathArg0])
puts '--Script Completed Successfully--'
