require 'optparse'
#AddToItemSet
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG') do |o| params[:pathArg0] = o end
opts.on('--searchArg0 ARG') do |o| params[:searchArg0] = o end
opts.on('--itemSetNameArg0 ARG') do |o| params[:itemSetNameArg0] = o end
opts.on('--deduplicationArg0 [ARG]') do |o| params[:deduplicationArg0] = o end
opts.on('--descriptionArg0 [ARG]') do |o| params[:descriptionArg0] = o end
opts.on('--deduplicateByArg0 ARG') do |o| params[:deduplicateByArg0] = o end
opts.on('--custodianRankingArg0 [ARG]') do |o| params[:custodianRankingArg0] = o end
opts.on('--orderArg0 [ARG]') do |o| params[:orderArg0] = o end
opts.on('--limitArg0 [ARG]') do |o| params[:limitArg0] = o end
end.parse!
puts params

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



AddToItemSet(utilities, params[:pathArg0], params[:searchArg0], params[:itemSetNameArg0], params[:deduplicationArg0], params[:descriptionArg0], params[:deduplicateByArg0], params[:custodianRankingArg0], params[:orderArg0], params[:limitArg0])
puts '--Script Completed Successfully--'
