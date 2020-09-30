using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
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
        public static RubyScriptStepFactory<NuixImportDocumentIds, Unit> Instance { get; } = new NuixImportDocumentIdsStepFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(7, 4);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET
        };

        /// <inheritdoc />
        public override string FunctionName => "ImportDocumentIds";

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
            sourceProductionSetsInData: pathArg == ""true"",
            dataPath: dataPathArg
        }

        failedItemsCount = productionSet.importDocumentIds(options)

        if failedItemsCount == 0
            puts ""All document ids imported successfully""
        else
            puts ""#{failedItemsCount} items failed to import""

    end

    the_case.close";
    }


    /// <summary>
    /// Imports the given document IDs into this production set. Only works if this production set has imported numbering.
    /// </summary>
    public sealed class NuixImportDocumentIds : RubyScriptStepUnit
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => NuixImportDocumentIdsStepFactory.Instance;


        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [StepProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IStep<string> CasePath { get; set; } = null!;

        /// <summary>
        /// Specifies that the source production set name(s) are contained in the document ID list.
        /// </summary>

        [Required]
        [StepProperty]
        [DefaultValueExplanation("false")]
        [RubyArgument("sourceProductionSetsInDataArg", 2)]
        public IStep<bool> AreSourceProductionSetsInData { get; set; } = new Constant<bool>(false);

        /// <summary>
        /// The production set to add results to.
        /// </summary>
        [Required]
        [StepProperty]
        [RubyArgument("productionSetNameArg", 3)]

        public IStep<string> ProductionSetName { get; set; } = null!;



        /// <summary>
        /// Specifies the file path of the document ID list.
        /// </summary>

        [Required]
        [StepProperty]
        [RubyArgument("dataPathArg", 4)]
        public IStep<string> DataPath { get; set; } = null!;
    }
}