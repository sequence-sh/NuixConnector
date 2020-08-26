using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes
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
        public override Version RequiredVersion { get; } = new Version(2, 16);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
            = new List<NuixFeature>
            {
                NuixFeature.ANALYSIS
            };
    }



    /// <summary>
    /// Searches a NUIX case with a particular search string and tags all files it finds.
    /// </summary>
    public sealed class NuixSearchAndTag : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixSearchAndTagProcessFactory.Instance;


        ///// <inheritdoc />
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public override string GetName() => $"Search and Tag with '{Tag}'";

        /// <summary>
        /// The tag to assign to found results.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IRunnableProcess<string> Tag { get; set; }


        /// <summary>
        /// The term to search for.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("*.txt")]
        public IRunnableProcess<string> SearchTerm { get; set; }

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        public IRunnableProcess<string> CasePath { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        internal override string ScriptText => @"
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

        /// <inheritdoc />
        public override string MethodName => "SearchAndTag";

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, IRunnableProcess? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("searchArg", SearchTerm, false);
            yield return ("tagArg", Tag, false);
        }
    }
}