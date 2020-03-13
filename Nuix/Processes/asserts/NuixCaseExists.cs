using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes.asserts
{
    /// <summary>
    /// Succeeds or fails depending on whether or not a particular case exists.
    /// Useful in Conditionals.
    /// </summary>
    public sealed class NuixCaseExists : RubyScriptAssertionProcess
    {
        /// <inheritdoc />
        protected override (bool, string?)? InterpretLine(string s)
        {
            bool? exists = s switch
            {
                "Case Exists" => true,
                "Case does not exist" => false,
                _ => null,
            };
            if (!exists.HasValue)
                return null;


            if (ShouldExist)
                return exists.Value ? (true, null) : (false, $"Case '{CasePath}' should exist but does not.");

            return exists.Value ? (false, $"Case '{CasePath}' should not exist but does.") : (true, null);
        }

        /// <inheritdoc />
        protected override string DefaultFailureMessage => "Could not confirm whether or not case exists";


        /// <inheritdoc />
        public override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
        }

        /// <inheritdoc />
        public override string GetName()
        {
            return ShouldExist ? "Case should exist" : "Case should not exist";
        }

        internal override string ScriptName => "DoesCaseExist.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
        }

        /// <summary>
        /// If true, asserts that the case does exist.
        /// If false, asserts that the case does not exist.
        /// </summary>
        [Required]
        [YamlMember(Order = 2)]
        public bool ShouldExist { get; set; } = true;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [YamlMember(Order = 3)]
        [ExampleValue("C:/Cases/MyCase")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        
    }
}