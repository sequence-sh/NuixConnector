using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.processes
{
    /// <summary>
    /// Checks whether or not a particular case exists.
    /// </summary>
    public sealed class NuixCheckCaseExists : RubyScriptAssertionProcess
    {
        /// <inheritdoc />
        protected override bool? InterpretLine(string s)
        {
            if (s.Equals("Case Exists"))
                return ShouldExist;

            if (s.Equals("Case does not exist"))
                return !ShouldExist;

            return null;
        }

        /// <inheritdoc />
        protected override string FailureMessage => ShouldExist ? "Case does not exist." : "Case exists";

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
        [DataMember]
        [YamlMember(Order = 2 )]
        public bool ShouldExist { get; set; }

        /// <summary>
        /// The case path to check
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        
    }
}