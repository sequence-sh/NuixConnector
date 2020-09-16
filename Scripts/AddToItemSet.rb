#AddToItemSet

requiredNuixVersion = '4.0'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	raise "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
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
	opts.on('--pathArg1 [ARG]') do |o| params[:pathArg1] = o end
	opts.on('--searchArg1 [ARG]') do |o| params[:searchArg1] = o end
	opts.on('--itemSetNameArg1 [ARG]') do |o| params[:itemSetNameArg1] = o end
	opts.on('--deduplicationArg1 [ARG]') do |o| params[:deduplicationArg1] = o end
	opts.on('--descriptionArg1 [ARG]') do |o| params[:descriptionArg1] = o end
	opts.on('--deduplicateByArg1 [ARG]') do |o| params[:deduplicateByArg1] = o end
	opts.on('--custodianRankingArg1 [ARG]') do |o| params[:custodianRankingArg1] = o end
	opts.on('--orderArg1 [ARG]') do |o| params[:orderArg1] = o end
	opts.on('--limitArg1 [ARG]') do |o| params[:limitArg1] = o end
end.parse!


def AddToItemSet(utilities,pathArg,searchArg,itemSetNameArg,deduplicationArg,descriptionArg,deduplicateByArg,custodianRankingArg,orderArg,limitArg)

    the_case = utilities.case_factory.open(pathArg)
    itemSet = the_case.findItemSetByName(itemSetNameArg)
    if(itemSet == nil)
        itemSetOptions = {}
        itemSetOptions[:deduplication] = deduplicationArg if deduplicationArg != nil
        itemSetOptions[:description] = descriptionArg if descriptionArg != nil
        itemSetOptions[:deduplicateBy] = deduplicateByArg if deduplicateByArg != nil
        itemSetOptions[:custodianRanking] = custodianRankingArg.split(",") if custodianRankingArg != nil
        itemSet = the_case.createItemSet(itemSetNameArg, itemSetOptions)

        puts "Item Set Created"
    else
        puts "Item Set Found"
    end

    puts "Searching"
    searchOptions = {}
    searchOptions[:order] = orderArg if orderArg != nil
    searchOptions[:limit] = limitArg.to_i if limitArg != nil
    items = the_case.search(searchArg, searchOptions)
    puts "#{items.length} found"
    itemSet.addItems(items)
    puts "items added"
    the_case.close
end


AddToItemSet(utilities, params[:pathArg1], params[:searchArg1], params[:itemSetNameArg1], params[:deduplicationArg1], params[:descriptionArg1], params[:deduplicateByArg1], params[:custodianRankingArg1], params[:orderArg1], params[:limitArg1])
puts '--Script Completed Successfully--'
