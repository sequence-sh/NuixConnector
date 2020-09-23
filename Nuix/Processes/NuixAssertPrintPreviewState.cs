using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Processes
{
    /// <summary>
    /// Checks the print preview state of the production set.
    /// </summary>
    public sealed class NuixAssertPrintPreviewStateProcessFactory : RubyScriptProcessFactory<NuixAssertPrintPreviewState, Unit>
    {
        private NuixAssertPrintPreviewStateProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixAssertPrintPreviewState, Unit> Instance { get; } = new NuixAssertPrintPreviewStateProcessFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(5, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET, NuixFeature.ANALYSIS
        };

        /// <inheritdoc />
        public override string FunctionName => "GetPrintPreviewState";

        /// <inheritdoc />
        public override string RubyFunctionText =>
        @"
    the_case = utilities.case_factory.open(pathArg)
    productionSet = the_case.findProductionSetByName(productionSetNameArg)

    if(productionSet == nil)
        the_case.close
        raise 'Production Set Not Found'
        
        exit
    else
        r = productionSet.getPrintPreviewState()
        the_case.close

        if r.downcase == expectedStateArg.downcase
            puts ""Print preview state was #{r}, as expected.""
        else
            raise ""Print preview state was #{r}, but expected #{expectedStateArg}""
            exit
        end
    end";

    }


    /// <summary>
    /// Checks the print preview state of the production set.
    /// </summary>
    public sealed class NuixAssertPrintPreviewState : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<Unit> RubyScriptProcessFactory => NuixAssertPrintPreviewStateProcessFactory.Instance;

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
        /// The expected print preview state of the production set;
        /// </summary>
        [RunnableProcessProperty]
        [DefaultValueExplanation(nameof(PrintPreviewState.All))]
        [RubyArgument("expectedStateArg", 3)]

        public IRunnableProcess<PrintPreviewState> ExpectedState { get; set; } = new Constant<PrintPreviewState>(PrintPreviewState.All);
    }
}