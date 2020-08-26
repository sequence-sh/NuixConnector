#AddToProductionSet

requiredNuixVersion = '7.2'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

requiredFeatures = Array['PRODUCTION_SET']
requiredFeatures.each do |feature|
	if !utilities.getLicence().hasFeature(feature)
		puts "Nuix Feature #{feature} is required but not available."
		exit
	end
end

require 'optparse'
params = {}
OptionParser.new do |opts|
	opts.on('--pathArg0 ARG') do |o| params[:pathArg0] = o end
	opts.on('--searchArg0 ARG') do |o| params[:searchArg0] = o end
	opts.on('--productionSetNameArg0 ARG') do |o| params[:productionSetNameArg0] = o end
	opts.on('--descriptionArg0 [ARG]') do |o| params[:descriptionArg0] = o end
	opts.on('--productionProfileNameArg0 [ARG]') do |o| params[:productionProfileNameArg0] = o end
	opts.on('--productionProfilePathArg0 [ARG]') do |o| params[:productionProfilePathArg0] = o end
	opts.on('--orderArg0 [ARG]') do |o| params[:orderArg0] = o end
	opts.on('--limitArg0 [ARG]') do |o| params[:limitArg0] = o end
end.parse!


def AddToProductionSet(utilities,pathArg,searchArg,productionSetNameArg,descriptionArg,productionProfileNameArg,productionProfilePathArg,orderArg,limitArg)

    the_case = utilities.case_factory.open(pathArg)
    puts "Searching"

    searchOptions = {}
    searchOptions[:order] = orderArg if orderArg != nil
    searchOptions[:limit] = limitArg.to_i if limitArg != nil

    items = the_case.search(searchArg, searchOptions)
    puts "#{items.length} items found"

    productionSet = the_case.findProductionSetByName(productionSetNameArg)
    if(productionSet == nil)
        options = {}
        options[:description] = descriptionArg.to_i if descriptionArg != nil
        productionSet = the_case.newProductionSet(productionSetNameArg, options)

        if productionProfileNameArg != nil
            productionSet.setProductionProfile(productionProfileNameArg)
        elsif productionProfilePathArg != nil
            profileBuilder = utilities.getProductionProfileBuilder()
            profile = profileBuilder.load(productionProfilePathArg)

            if profile == nil
                puts "Could not find processing profile at #{productionProfilePathArg}"
                exit
            end

            productionSet.setProductionProfileObject(profile)
        else
            puts 'No production profile set'
            exit
        end

        puts "Production Set Created"
    else
        puts "Production Set Found"
    end

    if items.length > 0
        productionSet.addItems(items)
        puts "Items added to production set"
    else
        puts "No items to add to production Set"
    end

    the_case.close
end



AddToProductionSet(utilities, params[:pathArg0], params[:searchArg0], params[:productionSetNameArg0], params[:descriptionArg0], params[:productionProfileNameArg0], params[:productionProfilePathArg0], params[:orderArg0], params[:limitArg0])
puts '--Script Completed Successfully--'
