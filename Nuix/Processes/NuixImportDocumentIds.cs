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
    /// Imports the given document IDs into this production set. Only works if this production set has imported numbering.
    /// </summary>
    public sealed class NuixImportDocumentIdsProcessFactory : RubyScriptProcessFactory<NuixImportDocumentIds, Unit>
    {
        private NuixImportDocumentIdsProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixImportDocumentIds, Unit> Instance { get; } = new NuixImportDocumentIdsProcessFactory();

        /// <inheritdoc />
        public override Version RequiredVersion { get; } = new Version(7, 4);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET
        };
    }


    /// <summary>
    /// Imports the given document IDs into this production set. Only works if this production set has imported numbering.
    /// </summary>
    public sealed class NuixImportDocumentIds : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixImportDocumentIdsProcessFactory.Instance;


        ///// <inheritdoc />
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public override string GetName() => $"Add document ids to production set.";

        /// <summary>
        /// The production set to add results to.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IRunnableProcess<string> ProductionSetName { get; set; }

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        public IRunnableProcess<string> CasePath { get; set; }

        /// <summary>
        /// Specifies that the source production set name(s) are contained in the document ID list.
        /// </summary>

        [Required]
        [RunnableProcessProperty]
        [DefaultValueExplanation("false")]
        public IRunnableProcess<bool> AreSourceProductionSetsInData { get; set; } = new Constant<bool>(false);

        /// <summary>
        /// Specifies the file path of the document ID list.
        /// </summary>

        [Required]
        [RunnableProcessProperty]
        public IRunnableProcess<string> DataPath { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        internal override string ScriptText => @"

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

        /// <inheritdoc />
        public override string MethodName => "ImportDocumentIds";

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, IRunnableProcess? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("sourceProductionSetsInDataArg", AreSourceProductionSetsInData, false);
            yield return ("productionSetNameArg", ProductionSetName, false);
            yield return ("dataPathArg", DataPath, false);
        }
    }
}