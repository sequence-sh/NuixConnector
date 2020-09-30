using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Returns the number of items matching a particular search term
    /// </summary>
    public sealed class NuixCountItemsStepFactory : RubyScriptStepFactory<NuixCountItems, int>
    {
        private NuixCountItemsStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<NuixCountItems, int> Instance { get; } = new NuixCountItemsStepFactory();

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
    public sealed class NuixCountItems : RubyScriptStepTyped<int>
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<int> RubyScriptStepFactory => NuixCountItemsStepFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [StepProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IStep<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The search term to count.
        /// </summary>
        [Required]
        [Example("*.txt")]
        [StepProperty]
        [RubyArgument("searchArg", 2)]
        public IStep<string> SearchTerm { get; set; } = null!;

        /// <inheritdoc />
        public override bool TryParse(string s, out int result) => int.TryParse(s, out result);
    }
}