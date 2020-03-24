def AddToItemSet(pathArg, productionSetNameArg, deduplicationArg, descriptionArg, deduplicateByArg, custodianRankingArg, orderArg, limitArg, searchArg)

    
    
    the_case = utilities.case_factory.open(pathArg)

    itemSet = the_case.findItemSetByName(productionSetNameArg)

    if(itemSet == nil)
        itemSetOptions = {}

        itemSetOptions[:deduplication] = deduplicationArg if deduplicationArg != nil
        itemSetOptions[:description] = descriptionArg if descriptionArg != nil
        itemSetOptions[:deduplicateBy] = deduplicateByArg if deduplicateByArg != nil
        itemSetOptions[:custodianRanking] = custodianRankingArg.split(",") if custodianRankingArg != nil

        itemSet = the_case.createItemSet(productionSetNameArg, itemSetOptions)
        
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