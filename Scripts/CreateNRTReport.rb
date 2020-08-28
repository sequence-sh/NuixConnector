#CreateNRTReport

requiredNuixVersion = '7.4'
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
	opts.on('--pathArg1 [ARG]') do |o| params[:pathArg1] = o end
	opts.on('--nrtPathArg1 [ARG]') do |o| params[:nrtPathArg1] = o end
	opts.on('--outputFormatArg1 [ARG]') do |o| params[:outputFormatArg1] = o end
	opts.on('--outputPathArg1 [ARG]') do |o| params[:outputPathArg1] = o end
	opts.on('--localResourcesUrlArg1 [ARG]') do |o| params[:localResourcesUrlArg1] = o end
end.parse!


def CreateNRTReport(utilities,pathArg,nrtPathArg,outputFormatArg,outputPathArg,localResourcesUrlArg)

    the_case = utilities.case_factory.open(pathArg)
    puts 'Generating NRT Report:'

    reportGenerator = utilities.getReportGenerator();
    reportContext = {
    'NUIX_USER' => 'Mark',
    'NUIX_APP_NAME' => 'AppName',
    'NUIX_REPORT_TITLE' => 'ReportTitle',
    'NUIX_APP_VERSION' => NUIX_VERSION,
    'LOCAL_RESOURCES_URL' => localResourcesUrlArg,
    'currentCase' => the_case,
    'utilities' => $utilities,
    'dedupeEnabled' => true
    }

    reportGenerator.generateReport(
    nrtPathArg,
    reportContext.to_java,
    outputFormatArg,
    outputPathArg
    )

    the_case.close
end


CreateNRTReport(utilities, params[:pathArg1], params[:nrtPathArg1], params[:outputFormatArg1], params[:outputPathArg1], params[:localResourcesUrlArg1])
puts '--Script Completed Successfully--'
