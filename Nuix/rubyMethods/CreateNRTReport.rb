def CreateNRTReport(pathArg, localResourcesUrlArg, NRTPathArg, OutputFormatArg, OutputPathArg)

    
    
    the_case = utilities.case_factory.open(pathArg)

    puts "Generating Report:"


    reportGenerator = utilities.getReportGenerator();

    reportContext = {
    "NUIX_USER" => "Mark",
    "NUIX_APP_NAME" => "AppName",
    "NUIX_REPORT_TITLE" => "ReportTitle",
    "NUIX_APP_VERSION" => NUIX_VERSION,
    "LOCAL_RESOURCES_URL" => localResourcesUrlArg,
    "currentCase" => the_case,
    "utilities" => $utilities,
    "dedupeEnabled" => true
    }

    reportGenerator.generateReport(
    NRTPathArg,
    reportContext.to_java,
    OutputFormatArg,
    OutputPathArg
    )


    the_case.close
    
    
end