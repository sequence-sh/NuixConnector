using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Enums;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Checks the print preview state of the production set.
    /// </summary>
    public sealed class NuixAssertPrintPreviewStateStepFactory : RubyScriptStepFactory<NuixAssertPrintPreviewState, Unit>
    {
        private NuixAssertPrintPreviewStateStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<NuixAssertPrintPreviewState, Unit> Instance { get; } = new NuixAssertPrintPreviewStateStepFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new(5, 2);

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
    the_case = $utilities.case_factory.open(pathArg)
    productionSet = the_case.findProductionSetByName(productionSetNameArg)

    if(productionSet == nil)
        the_case.close
        raise 'Production Set Not Found'
        
        exit
    else
        r = productionSet.getPrintPreviewState()
        the_case.close

        if r.to_s.downcase == expectedStateArg.to_s.downcase
            log ""Print preview state was #{r}, as expected.""
        else
            raise ""Print preview state was #{r}, but expected #{expectedStateArg}""
            exit
        end
    end";

    }


    /// <summary>
    /// Checks the print preview state of the production set.
    /// </summary>
    public sealed class NuixAssertPrintPreviewState : RubyScriptStepBase<Unit>
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => NuixAssertPrintPreviewStateStepFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [StepProperty(1)]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IStep<StringStream> CasePath { get; set; } = null!;

        /// <summary>
        /// The production set to reorder.
        /// </summary>
        [Required]
        [StepProperty(2)]
        [RubyArgument("productionSetNameArg", 2)]
        public IStep<StringStream> ProductionSetName { get; set; } = null!;

        /// <summary>
        /// The expected print preview state of the production set;
        /// </summary>
        [StepProperty(3)]
        [DefaultValueExplanation(nameof(PrintPreviewState.All))]
        [RubyArgument("expectedStateArg", 3)]

        public IStep<PrintPreviewState> ExpectedState { get; set; } = new EnumConstant<PrintPreviewState>(PrintPreviewState.All);
    }
}