using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Creates a report for a Nuix case.
    /// The report is in csv format.
    /// The headers are 'Custodian', 'Type', 'Value', and 'Count'.
    /// The different types are: 'Kind', 'Type', 'Tag', and 'Address'.
    /// Use this inside a WriteFile process to write it to a file.
    /// </summary>
    public sealed class NuixCreateReportProcessFactory : RubyScriptProcessFactory<NuixCreateReport, string>
    {
        private NuixCreateReportProcessFactory(){ }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixCreateReport, string> Instance { get; } = new NuixCreateReportProcessFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(6, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.ANALYSIS
        };

        /// <inheritdoc />
        public override string FunctionName => "CreateReport";

        /// <inheritdoc />
        public override string RubyFunctionText =>
            @"
    the_case = utilities.case_factory.open(casePathArg)

    puts ""Generating Report:""
    allItems = the_case.searchUnsorted("""")
    results = Hash.new { |h, k| h[k] = Hash.new { |hh, kk| hh[kk] = Hash.new{0} } }

    allItems.each do |i|
        custodians = [""*""]
        custodians << i.getCustodian() if i.getCustodian() != nil

        custodians.each do |c|
            hash = results[c]

            kindsHash = hash[:kind]
            kindsHash[""*""] += 1
            kindsHash[i.getKind().getName()]  += 1

            typesHash = hash[:type]
            typesHash[i.getType().getName()] += 1

            tagsHash = hash[:tag]
            i.getTags().each do |t|
                tagsHash[t] += 1
            end

            language = i.getLanguage()
            if language != nil
                languageHash = hash[:language]
                languageHash[language] += 1
            end

            communication = i.getCommunication()
            if communication != nil

                from = communication.getFrom()
                to = communication.getTo()
                cc = communication.getCc()
                bcc = communication.getBcc()

                addressesHash = hash[:address]
                from.each { |a|  addressesHash[a] += 1} if from != nil
                to.each { |a|  addressesHash[a] += 1} if to != nil
                cc.each { |a|  addressesHash[a] += 1} if cc != nil
                bcc.each { |a|  addressesHash[a] += 1} if bcc != nil
            end
        end
    end

    puts ""Created results for #{allItems.length} items""

    text = ""Custodian\tType\tValue\tCount""

    puts ""#{results.length - 1} custodians""
    results.each do |custodian, hash1|
        hash1.each do |type, hash2|
            puts ""#{custodian} has #{hash2.length} #{type}s"" if custodian != ""*""
            hash2.sort_by{|value, count| -count}.each do |value, count|
                text <<  ""\n#{custodian}\t#{type}\t#{value}\t#{count}""
            end
        end
    end

    the_case.close
    return text;";
    }


    /// <summary>
    /// Creates a report for a Nuix case.
    /// The report is in csv format.
    /// The headers are 'Custodian', 'Type', 'Value', and 'Count'.
    /// The different types are: 'Kind', 'Type', 'Tag', and 'Address'.
    /// Use this inside a WriteFile process to write it to a file.
    /// </summary>
    public sealed class NuixCreateReport : RubyScriptProcessTyped<string>
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<string> RubyScriptProcessFactory => NuixCreateReportProcessFactory.Instance;


        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("casePathArg", 1)]
        public IRunnableProcess<string> CasePath { get; set; } = null!;


        /// <inheritdoc />
        public override bool TryParse(string s, out string result)
        {
            result = s;
            return true;
        }
    }
}