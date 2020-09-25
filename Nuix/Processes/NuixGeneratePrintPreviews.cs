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
    /// Generates print previews for items in a production set.
    /// </summary>
    public sealed class NuixGeneratePrintPreviewsProcessFactory : RubyScriptProcessFactory<NuixGeneratePrintPreviews, Unit>
    {
        private NuixGeneratePrintPreviewsProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixGeneratePrintPreviews, Unit> Instance { get; } = new NuixGeneratePrintPreviewsProcessFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(5, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET
        };

        /// <inheritdoc />
        public override string FunctionName => "GeneratePrintPreviews";


        /// <inheritdoc />
        public override string RubyFunctionText => @"
    the_case = utilities.case_factory.open(pathArg)

    productionSet = the_case.findProductionSetByName(productionSetNameArg)

    if(productionSet == nil)
        puts ""Production Set Not Found""
    else
        puts ""Production Set Found""

        options = {}

        resultMap = productionSet.generatePrintPreviews(options)

        puts ""Print previews generated""
    end

    the_case.close";
    }

    /// <summary>
    /// Generates print previews for items in a production set.
    /// </summary>
    public sealed class NuixGeneratePrintPreviews : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<Unit> RubyScriptProcessFactory => NuixGeneratePrintPreviewsProcessFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>

        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IRunnableProcess<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The production set to generate print previews for.
        /// </summary>

        [Required]
        [RunnableProcessProperty]
        [RubyArgument("productionSetNameArg", 2)]

        public IRunnableProcess<string> ProductionSetName { get; set; } = null!;
    }
}