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
    /// Searches a NUIX case with a particular search string and assigns all files it finds to a particular custodian.
    /// </summary>
    public sealed class NuixAssignCustodianFactory : RubyScriptProcessFactory<NuixAssignCustodian, Unit>
    {
        private NuixAssignCustodianFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixAssignCustodian, Unit> Instance { get; } = new NuixAssignCustodianFactory();

        /// <inheritdoc />
        public override Version RequiredVersion { get; } = new Version(3, 6);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
            = new List<NuixFeature>
            {
                NuixFeature.ANALYSIS
            };
    }


    /// <summary>
    /// Searches a NUIX case with a particular search string and assigns all files it finds to a particular custodian.
    /// </summary>
    public sealed class NuixAssignCustodian : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixAssignCustodianFactory.Instance;


        ///// <inheritdoc />
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public override string GetName() => $"Assign Custodian '{Custodian}'";

        /// <summary>
        /// The custodian to assign to found results.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IRunnableProcess<string> Custodian { get; set; }


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
        if i.getCustodian != custodianArg
            added = i.assignCustodian(custodianArg)
            j += 1
        end
    }

    puts ""#{j} items assigned to custodian #{custodianArg}""
    the_case.close";

        /// <inheritdoc />
        public override string MethodName => "AssignCustodian";

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, IRunnableProcess? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("searchArg", SearchTerm, false);
            yield return ("custodianArg", Custodian, false);
        }
    }
}