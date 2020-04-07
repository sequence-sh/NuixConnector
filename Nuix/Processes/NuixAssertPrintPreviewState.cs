using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.enums;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Checks the print preview state of the production set.
    /// </summary>
    public sealed class NuixAssertPrintPreviewState : RubyScriptProcess
    {
        /// <inheritdoc />
        protected override NuixReturnType ReturnType => NuixReturnType.Unit;

        /// <inheritdoc />
        public override string GetName()
        {
            return $"Assert preview state is {ExpectedState}";
        }

        /// <summary>
        /// The expected print preview state of the production set;
        /// </summary>
        [YamlMember(Order = 2)] 
        public PrintPreviewState ExpectedState { get; set; } = PrintPreviewState.All;

        /// <summary>
        /// The production set to reorder.
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
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        
        /// <inheritdoc />
        internal override string ScriptText =>
        @"
    the_case = utilities.case_factory.open(pathArg)
    productionSet = the_case.findProductionSetByName(productionSetNameArg)

    if(productionSet == nil)        
        puts 'Production Set Not Found'
        the_case.close
        exit
    else 
        r = productionSet.getPrintPreviewState()
        the_case.close

        if r == expectedStateArg
            puts ""Print preview state was #{r}, as expected.""
        else
            puts ""Print preview state was #{r}, but expected #{expectedStateArg}""
            exit
        end
    end";

        /// <inheritdoc />
        internal override string MethodName => "GetPrintPreviewState";

        /// <inheritdoc />
        internal override Version RequiredVersion { get; } = new Version(5,2);

        /// <inheritdoc />
        internal override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET, NuixFeature.ANALYSIS
        };

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("productionSetNameArg", ProductionSetName, false);
            yield return ("expectedStateArg", ExpectedState.ToString().ToLower(), false);
        }
    }
}