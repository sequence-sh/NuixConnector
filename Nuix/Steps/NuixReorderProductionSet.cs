using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Reorders and renumbers the items in a production set.
    /// </summary>
    public sealed class NuixReorderProductionSetStepFactory : RubyScriptStepFactory<NuixReorderProductionSet, Unit>
    {
        private NuixReorderProductionSetStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<NuixReorderProductionSet, Unit> Instance { get; } = new NuixReorderProductionSetStepFactory();

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
    public sealed class NuixReorderProductionSet : RubyScriptStepUnit
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => NuixReorderProductionSetStepFactory.Instance;




        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [StepProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IStep<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The production set to reorder.
        /// </summary>
        [Required]
        [StepProperty]
        [RubyArgument("productionSetNameArg", 2)]

        public IStep<string> ProductionSetName { get; set; } = null!;

        /// <summary>
        /// The method of sorting items during the renumbering.
        /// </summary>
        [Required]
        [StepProperty]
        [DefaultValueExplanation(nameof(ProductionSetSortOrder.Position))]
        [RubyArgument("sortOrderArg", 3)]
        public IStep<ProductionSetSortOrder> SortOrder { get; set; } = new Constant<ProductionSetSortOrder>(ProductionSetSortOrder.Position);


    }
}