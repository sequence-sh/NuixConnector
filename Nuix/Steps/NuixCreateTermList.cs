using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Creates a list of all terms appearing in the case and their frequencies.
    /// The report is in CSV format. The headers are 'Term' and 'Count'
    /// Use this inside a WriteFile step to write it to a file.
    /// </summary>
    public sealed class NuixCreateTermListStepFactory : RubyScriptStepFactory<NuixCreateTermList, string>
    {
        private NuixCreateTermListStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<NuixCreateTermList, string> Instance { get; } = new NuixCreateTermListStepFactory();

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
    /// Use this inside a WriteFile step to write it to a file.
    /// </summary>
    public sealed class NuixCreateTermList : RubyScriptStepBase<string>
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<string> RubyScriptStepFactory => NuixCreateTermListStepFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [StepProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("casePathArg", 1)]
        public IStep<string> CasePath { get; set; } = null!;
    }
}