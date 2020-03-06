require 'optparse'

hash_options = {}
OptionParser.new do |opts|
  opts.banner = "Usage: your_app [options]"
  opts.on('-p [ARG]', '--path [ARG]', "Case Path") do |v|
    hash_options[:pathArg] = v
  end
  opts.on('-n [ARG]', '--nrtPath [ARG]', "NRT Path") do |v|
    hash_options[:NRTPathArg] = v
  end
  opts.on('-f [ARG]', '--outputFormat [ARG]', "Output Format") do |v|
    hash_options[:OutputFormatArg] = v
  end
  opts.on('-l [ARG]', '--localResourcesUrl [ARG]', "Local Resources Url") do |v|
    hash_options[:localResourcesUrlArg] = v
  end
  opts.on('-o [ARG]', '--outputPath [ARG]', "Output Path") do |v|
    hash_options[:OutputPathArg] = v
  end
  opts.on('-h', '--help', 'Display this help') do 
    puts opts
    exit
  end

end.parse!

requiredArguments = [:pathArg, :NRTPathArg, :OutputFormatArg, :OutputPathArg, :localResourcesUrlArg]

unless requiredArguments.all? {|a| hash_options[a] != nil}
    puts "Missing arguments #{(requiredArguments.select {|a| hash_options[a] == nil}).to_s}"

else
    puts "Opening Case"
    
    the_case = utilities.case_factory.open(hash_options[:pathArg])

    puts "Generating Report:"


    reportGenerator = utilities.getReportGenerator();

    reportContext = {
    "NUIX_USER" => "Mark",
    "NUIX_APP_NAME" => "AppName",
    "NUIX_REPORT_TITLE" => "ReportTitle",
    "NUIX_APP_VERSION" => NUIX_VERSION,
    "LOCAL_RESOURCES_URL" => hash_options[:localResourcesUrlArg],
    "currentCase" => $the_case,
    "utilities" => $utilities,
    "dedupeEnabled" => true
    }

    reportGenerator.generateReport(
    hash_options[:NRTPathArg],
    reportContext.to_java,
    hash_options[:OutputFormatArg],
    hash_options[:OutputPathArg]
    )


    the_case.close
    puts "Case Closed"
    
end