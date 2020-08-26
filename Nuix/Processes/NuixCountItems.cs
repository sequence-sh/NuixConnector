using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes
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
        public override Version RequiredVersion { get; } = new Version(3, 4);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>();
    }

    /// <summary>
    /// Returns the number of items matching a particular search term
    /// </summary>
    public sealed class NuixCountItems : RubyScriptProcessTyped<int>
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixCountItemsProcessFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IRunnableProcess<string> CasePath { get; set; }

        /// <summary>
        /// The search term to count.
        /// </summary>
        [Required]
        [Example("*.txt")]
        [RunnableProcessProperty]
        public IRunnableProcess<string> SearchTerm { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        internal override string ScriptText => @"
    the_case = utilities.case_factory.open(pathArg)
    searchOptions = {}
    count = the_case.count(searchArg, searchOptions)
    the_case.close
    puts ""#{count} found matching '#{searchArg}'""
    return count";

        /// <inheritdoc />
        public override string MethodName => "CountItems";

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, IRunnableProcess? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("searchArg", SearchTerm, false);
        }

        ///// <inheritdoc />
        //public override string GetName() => "Count Items";

        /// <inheritdoc />
        public override bool TryParse(string s, out int result) => int.TryParse(s, out result);
    }
}