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
  opts.on('-o [ARG]', '--outputPath [ARG]', "Output Path") do |v|
    hash_options[:OutputPathArg] = v
  end
  opts.on('-h', '--help', 'Display this help') do 
    puts opts
    exit
  end

end.parse!

requiredArguments = [:pathArg, :NRTPathArg, :OutputFormatArg, :OutputPathArg]

unless requiredArguments.all? {|a| hash_options[a] != nil}
    puts "Missing arguments #{(requiredArguments.select {|a| hash_options[a] == nil}).to_s}"

else
    puts "Opening Case"
    
    the_case = utilities.case_factory.open(hash_options[:pathArg])

    puts "Generating Report:"


    reportGenerator = utilities.getReportGenerator();

    reportContext = {
    "currentCase" => the_case,
    "utilities" => utilities
    }

    reportGenerator.generateReport(
    hash_options[:NRTPathArg],
    reportContext,
    hash_options[:OutputFormatArg],
    hash_options[:OutputPathArg]
    )


    the_case.close
    puts "Case Closed"
    
end