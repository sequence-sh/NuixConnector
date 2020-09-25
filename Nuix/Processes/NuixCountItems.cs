using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Processes
{
    /// <summary>
    /// Returns the number of items matching a particular search term
    /// </summary>
    public sealed class NuixCountItemsProcessFactory : RubyScriptProcessFactory<NuixCountItems, int>
    {
        private NuixCountItemsProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixCountItems, int> Instance { get; } = new NuixCountItemsProcessFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(3, 4);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>();

        /// <inheritdoc />
        public override string FunctionName => "CountItems";

        /// <inheritdoc />
        public override string RubyFunctionText => @"
    the_case = utilities.case_factory.open(pathArg)
    searchOptions = {}
    count = the_case.count(searchArg, searchOptions)
    the_case.close
    puts ""#{count} found matching '#{searchArg}'""
    return count";
    }

    /// <summary>
    /// Returns the number of items matching a particular search term
    /// </summary>
    public sealed class NuixCountItems : RubyScriptProcessTyped<int>
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<int> RubyScriptProcessFactory => NuixCountItemsProcessFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessPropertyAttribute]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IRunnableProcess<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The search term to count.
        /// </summary>
        [Required]
        [Example("*.txt")]
        [RunnableProcessPropertyAttribute]
        [RubyArgument("searchArg", 2)]
        public IRunnableProcess<string> SearchTerm { get; set; } = null!;

        /// <inheritdoc />
        public override bool TryParse(string s, out int result) => int.TryParse(s, out result);
    }
}