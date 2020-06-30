using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Annotates a document ID list to add production set names to it.
    /// </summary>
    internal class NuixAnnotateDocumentIdList : RubyScriptProcess
    {
        /// <inheritdoc />
        protected override NuixReturnType ReturnType => NuixReturnType.Unit;

        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName() => $"Annotates a document ID list";


        /// <summary>
        /// The production set to get names from.
        /// </summary>
        
        [Required]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string ProductionSetName { get; set; }

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [YamlMember(Order = 4)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }

        /// <summary>
        /// Specifies the file path of the document ID list.
        /// </summary>
        [Required]
        [YamlMember(Order = 5)]
        public string DataPath { get; set; }

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
        internal override string MethodName => "AnnotateDocumentIds";

        /// <inheritdoc />
        internal override Version RequiredVersion { get; } = new Version(7,4);

        /// <inheritdoc />
        internal override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET
        };

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("productionSetNameArg", ProductionSetName, false);
            yield return ("dataPathArg", DataPath, false);
        }
    }
}