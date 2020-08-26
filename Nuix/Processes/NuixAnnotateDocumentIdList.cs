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
        public override Version RequiredVersion { get; } = new Version(7, 4);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET
        };
    }


    /// <summary>
    /// Annotates a document ID list to add production set names to it.
    /// </summary>
    public class NuixAnnotateDocumentIdList : RubyScriptProcessUnit
    {

        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixAnnotateDocumentIdListProcessFactory.Instance;


        ///// <summary>
        ///// The name of this process
        ///// </summary>
        //public override string GetName() => $"Annotates a document ID list";


        /// <summary>
        /// The production set to get names from.
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
            dataPath: dataPathArg
        }
        resultMap = productionSet.annotateDocumentIdList(options)
        puts resultMap
    end

    the_case.close";

        /// <inheritdoc />
        public override string MethodName => "AnnotateDocumentIds";

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, IRunnableProcess? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("productionSetNameArg", ProductionSetName, false);
            yield return ("dataPathArg", DataPath, false);
        }
    }
}