using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Connectors.Nuix.Processes
{
    /// <summary>
    /// Searches a NUIX case with a particular search string and tags all files it finds.
    /// </summary>
    public sealed class NuixSearchAndTagProcessFactory : RubyScriptProcessFactory<NuixSearchAndTag, Unit>
    {
        private NuixSearchAndTagProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixSearchAndTag, Unit> Instance { get; } = new NuixSearchAndTagProcessFactory();

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
    public sealed class NuixSearchAndTag : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<Unit> RubyScriptProcessFactory => NuixSearchAndTagProcessFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessPropertyAttribute]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IRunnableProcess<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The term to search for.
        /// </summary>
        [Required]
        [RunnableProcessPropertyAttribute]
        [Example("*.txt")]
        [RubyArgument("searchArg", 2)]
        public IRunnableProcess<string> SearchTerm { get; set; } = null!;

        /// <summary>
        /// The tag to assign to found results.
        /// </summary>
        [Required]
        [RunnableProcessPropertyAttribute]
        [RubyArgument("tagArg", 3)]
        public IRunnableProcess<string> Tag { get; set; }= null!;
    }
}