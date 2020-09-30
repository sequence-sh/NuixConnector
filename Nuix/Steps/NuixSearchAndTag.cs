using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Searches a NUIX case with a particular search string and tags all files it finds.
    /// </summary>
    public sealed class NuixSearchAndTagStepFactory : RubyScriptStepFactory<NuixSearchAndTag, Unit>
    {
        private NuixSearchAndTagStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<NuixSearchAndTag, Unit> Instance { get; } = new NuixSearchAndTagStepFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(2, 16);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
            = new List<NuixFeature>
            {
                NuixFeature.ANALYSIS
            };

        /// <inheritdoc />
        public override string FunctionName => "SearchAndTag";

        /// <inheritdoc />
        public override string RubyFunctionText => @"
    the_case = utilities.case_factory.open(pathArg)
    puts ""Searching for '#{searchArg}'""

    searchOptions = {}
    items = the_case.search(searchArg, searchOptions)
    puts ""#{items.length} found""

    j = 0

    items.each {|i|
       added = i.addTag(tagArg)
       j += 1 if added
    }

    puts ""#{j} items tagged with #{tagArg}""
    the_case.close";
    }



    /// <summary>
    /// Searches a NUIX case with a particular search string and tags all files it finds.
    /// </summary>
    public sealed class NuixSearchAndTag : RubyScriptStepUnit
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => NuixSearchAndTagStepFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [StepProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IStep<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The term to search for.
        /// </summary>
        [Required]
        [StepProperty]
        [Example("*.txt")]
        [RubyArgument("searchArg", 2)]
        public IStep<string> SearchTerm { get; set; } = null!;

        /// <summary>
        /// The tag to assign to found results.
        /// </summary>
        [Required]
        [StepProperty]
        [RubyArgument("tagArg", 3)]
        public IStep<string> Tag { get; set; }= null!;
    }
}