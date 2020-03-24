using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.enums;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes.asserts
{
    /// <summary>
    /// Checks the print preview state of the production set.
    /// </summary>
    public sealed class NuixCheckPrintPreviewState : RubyScriptAssertionProcess
    {
        /// <inheritdoc />
        public override string GetName()
        {
            return $"Assert preview state is {ExpectedState}";
        }

        /// <inheritdoc />
        internal override string ScriptName => "GetPrintPreviewState.rb";

        /// <inheritdoc />
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-n", ProductionSetName);
        }

        /// <inheritdoc />
        protected override (bool success, string? failureMessage)? InterpretLine(string s)
        {
            var pps = s.ToLowerInvariant() switch
            {
                "all" => PrintPreviewState.All,
                "some" => PrintPreviewState.Some,
                "none" => PrintPreviewState.None,
                _ => null as PrintPreviewState?
            };

            if (pps == null) return null;

            if (pps == ExpectedState)
                return (true, null);

            return (false, $"Expected print preview state '{ExpectedState}' but was '{pps.Value}'");
        }

        /// <inheritdoc />
        protected override string DefaultFailureMessage => "Could not confirm print preview state";

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
    }
}