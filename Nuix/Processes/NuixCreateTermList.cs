using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Processes
{
    /// <summary>
    /// Creates a list of all terms appearing in the case and their frequencies.
    /// The report is in CSV format. The headers are 'Term' and 'Count'
    /// Use this inside a WriteFile process to write it to a file.
    /// </summary>
    public sealed class NuixCreateTermListProcessFactory : RubyScriptProcessFactory<NuixCreateTermList, string>
    {
        private NuixCreateTermListProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixCreateTermList, string> Instance { get; } = new NuixCreateTermListProcessFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(4, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>();


        /// <inheritdoc />
        public override string FunctionName => "CreateTermList";


        /// <inheritdoc />
        public override string RubyFunctionText => @"
    the_case = utilities.case_factory.open(casePathArg)

    puts ""Generating Report:""
    caseStatistics = the_case.getStatistics()
    termStatistics = caseStatistics.getTermStatistics("""", {""sort"" => ""on"", ""deduplicate"" => ""md5""}) #for some reason this takes strings rather than symbols
    #todo terms per custodian
    puts ""#{termStatistics.length} terms""

    text = ""Term\tCount""

    termStatistics.each do |term, count|
        text << ""\n#{term}\t#{count}""
    end

    the_case.close
    return text";
    }


    /// <summary>
    /// Creates a list of all terms appearing in the case and their frequencies.
    /// The report is in CSV format. The headers are 'Term' and 'Count'
    /// Use this inside a WriteFile process to write it to a file.
    /// </summary>
    public sealed class NuixCreateTermList : RubyScriptProcessTyped<string>
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<string> RubyScriptProcessFactory => NuixCreateTermListProcessFactory.Instance;

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