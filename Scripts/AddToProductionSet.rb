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
	opts.on('--pathArg1 [ARG]') do |o| params[:pathArg1] = o end
	opts.on('--searchArg1 [ARG]') do |o| params[:searchArg1] = o end
	opts.on('--productionSetNameArg1 [ARG]') do |o| params[:productionSetNameArg1] = o end
	opts.on('--descriptionArg1 [ARG]') do |o| params[:descriptionArg1] = o end
	opts.on('--productionProfileNameArg1 [ARG]') do |o| params[:productionProfileNameArg1] = o end
	opts.on('--productionProfilePathArg1 [ARG]') do |o| params[:productionProfilePathArg1] = o end
	opts.on('--orderArg1 [ARG]') do |o| params[:orderArg1] = o end
	opts.on('--limitArg1 [ARG]') do |o| params[:limitArg1] = o end
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


AddToProductionSet(utilities, params[:pathArg1], params[:searchArg1], params[:productionSetNameArg1], params[:descriptionArg1], params[:productionProfileNameArg1], params[:productionProfilePathArg1], params[:orderArg1], params[:limitArg1])
puts '--Script Completed Successfully--'
