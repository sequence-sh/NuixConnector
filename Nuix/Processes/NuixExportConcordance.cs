using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Utilities.Processes;
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


        /// <summary>
        /// The name of the metadata profile to use.
        /// </summary>
        [ExampleValue("MyMetadataProfile")]
        [DefaultValueExplanation("Use the Default profile.")]
        [YamlMember(Order = 3)]
        public string? MetadataProfileName { get; set; }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The name of the production set to export.
        /// </summary>
        [Required]
        [YamlMember(Order = 4)]
        public string ProductionSetName { get; set; }

        /// <summary>
        /// Where to export the Concordance to.
        /// </summary>
        [Required]
        [YamlMember(Order = 5)]
        public string ExportPath { get; set; }

        /// <summary>
        /// The path to the case.
        /// </summary>
        
        [Required]
        [YamlMember(Order = 6)]
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
    else
        batchExporter = utilities.createBatchExporter(exportPathArg)

        batchExporter.addLoadFile(""concordance"",{
        :metadataProfile => metadataProfileArg
		})

        batchExporter.addProduct(""native"", {
        :naming=> ""full"",
        :path => ""Native""
        })

        batchExporter.addProduct(""text"", {
        :naming=> ""full"",
        :path => ""Text""
        })


        puts 'Starting export.'
        batchExporter.exportItems(productionSet)        
        puts 'Export complete.'

    end

    the_case.close";

        /// <inheritdoc />
        internal override string MethodName => "ExportConcordance";

        /// <inheritdoc />
        internal override Version RequiredVersion { get; } = new Version(3,6);

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
            yield return ("metadataProfileArg", MetadataProfileName, true);
        }
    }
}
