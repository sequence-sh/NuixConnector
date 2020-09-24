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
    /// Annotates a document ID list to add production set names to it.
    /// </summary>
    public class NuixAnnotateDocumentIdListProcessFactory : RubyScriptProcessFactory<NuixAnnotateDocumentIdList, Unit>
    {
        private NuixAnnotateDocumentIdListProcessFactory() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static RubyScriptProcessFactory<NuixAnnotateDocumentIdList, Unit> Instance { get; } = new NuixAnnotateDocumentIdListProcessFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(7, 4);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET
        };

        /// <inheritdoc />
        public override string FunctionName => "AnnotateDocumentIds";

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
            dataPath: dataPathArg
        }
        resultMap = productionSet.annotateDocumentIdList(options)
        puts resultMap
    end

    the_case.close";

    }


    /// <summary>
    /// Annotates a document ID list to add production set names to it.
    /// </summary>
    public class NuixAnnotateDocumentIdList : RubyScriptProcessUnit
    {

        /// <inheritdoc />
        public override IRubyScriptProcessFactory<Unit> RubyScriptProcessFactory => NuixAnnotateDocumentIdListProcessFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IRunnableProcess<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The production set to get names from.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [RubyArgument("productionSetNameArg", 2)]
        public IRunnableProcess<string> ProductionSetName { get; set; }= null!;



        /// <summary>
        /// Specifies the file path of the document ID list.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [RubyArgument("dataPathArg", 3)]
        public IRunnableProcess<string> DataPath { get; set; }= null!;
    }
}