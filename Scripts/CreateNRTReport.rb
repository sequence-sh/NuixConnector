require 'optparse'
#CreateNRTReport
params = {}
OptionParser.new do |opts|
opts.on('--pathArg0 ARG')
opts.on('--nrtPathArg0 ARG')
opts.on('--outputFormatArg0 ARG')
opts.on('--outputPathArg0 ARG')
opts.on('--localResourcesUrlArg0 ARG')
end.parse!(into: params)
puts params

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



CreateNRTReport(utilities, params[:pathArg0], params[:nrtPathArg0], params[:outputFormatArg0], params[:outputPathArg0], params[:localResourcesUrlArg0])
puts '--Script Completed Successfully--'
