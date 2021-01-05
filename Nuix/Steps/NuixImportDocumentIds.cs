using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Parser;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Imports the given document IDs into this production set. Only works if this production set has imported numbering.
    /// </summary>
    public sealed class NuixImportDocumentIdsStepFactory : RubyScriptStepFactory<NuixImportDocumentIds, Unit>
    {
        private NuixImportDocumentIdsStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<NuixImportDocumentIds, Unit> Instance { get; } =
            new NuixImportDocumentIdsStepFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new (7, 4);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET
        };

        /// <inheritdoc />
        public override string FunctionName => "ImportDocumentIds";

        /// <inheritdoc />
        public override string RubyFunctionText => @"

    the_case = $utilities.case_factory.open(pathArg)

    productionSet = the_case.findProductionSetByName(productionSetNameArg)

    if(productionSet == nil)
        log ""Production Set Not Found""
    else
        log ""Production Set Found""

        options =
        {
            sourceProductionSetsInData: pathArg == ""true"",
            dataPath: dataPathArg
        }

        failedItemsCount = productionSet.importDocumentIds(options)

        if failedItemsCount == 0
            log ""All document ids imported successfully""
        else
            log ""#{failedItemsCount} items failed to import""

    end

    the_case.close";
    }

    /// <summary>
    /// Imports the given document IDs into this production set. Only works if this production set has imported numbering.
    /// </summary>
    public sealed class NuixImportDocumentIds : RubyScriptStepBase<Unit>
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => NuixImportDocumentIdsStepFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [StepProperty(1)]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        [Alias("Case")]
        public IStep<StringStream> CasePath { get; set; } = null!;

        /// <summary>
        /// The production set to add results to.
        /// </summary>
        [Required]
        [StepProperty(2)]
        [RubyArgument("productionSetNameArg", 2)]
        [Alias("ProductionSet")]
        public IStep<StringStream> ProductionSetName { get; set; } = null!;

        /// <summary>
        /// Specifies the file path of the document ID list.
        /// </summary>
        [Required]
        [StepProperty(3)]
        [RubyArgument("dataPathArg", 3)]
        [Alias("FromList")]
        public IStep<StringStream> DataPath { get; set; } = null!;

        /// <summary>
        /// Specifies that the source production set name(s) are contained in the document ID list.
        /// </summary>
        [StepProperty(4)]
        [DefaultValueExplanation("false")]
        [RubyArgument("sourceProductionSetsInDataArg", 4)]
        [Alias("SetNameInList")]
        public IStep<bool> AreSourceProductionSetsInData { get; set; } = new BoolConstant(false);
    }
}
