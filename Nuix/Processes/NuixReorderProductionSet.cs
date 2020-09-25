using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Connectors.Nuix.Processes
{
    /// <summary>
    /// Reorders and renumbers the items in a production set.
    /// </summary>
    public sealed class NuixReorderProductionSetProcessFactory : RubyScriptProcessFactory<NuixReorderProductionSet, Unit>
    {
        private NuixReorderProductionSetProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixReorderProductionSet, Unit> Instance { get; } = new NuixReorderProductionSetProcessFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(5, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET
        };

        /// <inheritdoc />
        public override string FunctionName => "RenumberProductionSet";


        /// <inheritdoc />
        public override string RubyFunctionText => @"
    the_case = utilities.case_factory.open(pathArg)

    productionSet = the_case.findProductionSetByName(productionSetNameArg)

    if(productionSet == nil)
        puts ""Production Set Not Found""
    else
        puts ""Production Set Found""

        options =
        {
            sortOrder: sortOrderArg
        }

        resultMap = productionSet.renumber(options)
        puts resultMap
    end

    the_case.close";
    }


    /// <summary>
    /// Reorders and renumbers the items in a production set.
    /// </summary>
    public sealed class NuixReorderProductionSet : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<Unit> RubyScriptProcessFactory => NuixReorderProductionSetProcessFactory.Instance;




        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IRunnableProcess<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The production set to reorder.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [RubyArgument("productionSetNameArg", 2)]

        public IRunnableProcess<string> ProductionSetName { get; set; } = null!;

        /// <summary>
        /// The method of sorting items during the renumbering.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [DefaultValueExplanation(nameof(ProductionSetSortOrder.Position))]
        [RubyArgument("sortOrderArg", 3)]
        public IRunnableProcess<ProductionSetSortOrder> SortOrder { get; set; } = new Constant<ProductionSetSortOrder>(ProductionSetSortOrder.Position);


    }
}