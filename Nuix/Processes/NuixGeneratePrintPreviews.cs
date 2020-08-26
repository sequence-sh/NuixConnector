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
        public override Version RequiredVersion { get; } = new Version(5, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET
        };
    }

    /// <summary>
    /// Generates print previews for items in a production set.
    /// </summary>
    public sealed class NuixGeneratePrintPreviews : RubyScriptProcess
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixGeneratePrintPreviewsProcessFactory.Instance;


        ///// <inheritdoc />
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public override string GetName() => "Generate print previews";

        /// <summary>
        /// The production set to generate print previews for.
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

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        internal override string ScriptText => @"
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

        /// <inheritdoc />
        internal override string MethodName => "GeneratePrintPreviews";

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, IRunnableProcess? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("productionSetNameArg", ProductionSetName, false);
        }
    }
}