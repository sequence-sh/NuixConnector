using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Exports Concordance for a particular production set.
    /// </summary>
    public sealed class NuixExportConcordance : RubyScriptProcess
    {
        /// <inheritdoc />
        protected override NuixReturnType ReturnType => NuixReturnType.Unit;

        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => $"Export {ProductionSetName}";

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The name of the production set to export.
        /// </summary>
        [Required]
        [YamlMember(Order = 5)]
        public string ProductionSetName { get; set; }

        /// <summary>
        /// Where to export the Concordance to.
        /// </summary>
        [Required]
        [YamlMember(Order = 6)]
        public string ExportPath { get; set; }

        /// <summary>
        /// The path to the case.
        /// </summary>
        
        [Required]
        [YamlMember(Order = 7)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        internal override string ScriptText =>
            @"
    the_case = utilities.case_factory.open(pathArg)

    productionSet = the_case.findProductionSetByName(productionSetNameArg)

    if productionSet == nil
        puts ""Could not find production set with name '#{productionSetNameArg.to_s}'""
    elsif productionSet.getProductionProfile == nil
        puts ""Production set '#{productionSetNameArg.to_s}' did not have a production profile set.""
    else
        batchExporter = utilities.createBatchExporter(exportPathArg)


        puts 'Starting export.'
        batchExporter.exportItems(productionSet)        
        puts 'Export complete.'

    end

    the_case.close";

        /// <inheritdoc />
        internal override string MethodName => "ExportConcordance";

        /// <inheritdoc />
        internal override Version RequiredVersion { get; } = new Version(7,2); //I'm checking the production profile here

        /// <inheritdoc />
        internal override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET,
            NuixFeature.EXPORT_ITEMS
        };

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("exportPathArg", ExportPath, false);
            yield return ("productionSetNameArg", ProductionSetName, false);
        }
    }
}
