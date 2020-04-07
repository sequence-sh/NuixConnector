require 'optparse'
#AddToItemSet
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG')
opts.on('--searchArg0 ARG')
opts.on('--itemSetNameArg0 ARG')
opts.on('--deduplicationArg0 [ARG]')
opts.on('--descriptionArg0 [ARG]')
opts.on('--deduplicateByArg0 ARG')
opts.on('--custodianRankingArg0 [ARG]')
opts.on('--orderArg0 [ARG]')
opts.on('--limitArg0 [ARG]')
end.parse!(into: params)
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
