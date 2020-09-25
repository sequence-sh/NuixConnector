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
        public override Version RequiredNuixVersion { get; } = new Version(3, 6);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }
            = new List<NuixFeature>
            {
                NuixFeature.ANALYSIS
            };


        /// <inheritdoc />
        public override string FunctionName => "AssignCustodian";

        /// <inheritdoc />
        public override string RubyFunctionText => @"
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
    }


    /// <summary>
    /// Searches a NUIX case with a particular search string and assigns all files it finds to a particular custodian.
    /// </summary>
    public sealed class NuixAssignCustodian : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<Unit> RubyScriptProcessFactory => NuixAssignCustodianFactory.Instance;

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
        /// The custodian to assign to found results.
        /// </summary>
        [Required]
        [RunnableProcessPropertyAttribute]
        [RubyArgument("custodianArg", 3)]
        public IRunnableProcess<string> Custodian { get; set; } = null!;
    }
}