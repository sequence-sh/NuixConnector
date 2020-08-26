#CreateReport

requiredNuixVersion = '6.2'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

requiredFeatures = Array['ANALYSIS']
requiredFeatures.each do |feature|
	if !utilities.getLicence().hasFeature(feature)
		puts "Nuix Feature #{feature} is required but not available."
		exit
	end
end

require 'optparse'
params = {}
OptionParser.new do |opts|
	opts.on('--casePathArg0 ARG') do |o| params[:casePathArg0] = o end
end.parse!


def CreateReport(utilities,casePathArg)

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
                text <<  "\n#{custodian}\t#{type}\t#{value}\t#{count}"
            end
        end
    end

    the_case.close
    return text;
end


def binToHex(s)
  suffix = s.to_s.each_byte.map { |b| b.to_s(16).rjust(2, '0') }.join('').upcase
  '0x' + suffix
end



result0 = CreateReport(utilities, params[:casePathArg0])
puts "--Final Result: #{binToHex(result0)}"
