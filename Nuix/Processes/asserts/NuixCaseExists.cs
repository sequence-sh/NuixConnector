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
    public sealed class NuixCaseExists : RubyScriptProcess
    {
        /// <inheritdoc />
        public override string GetName()
        {
            return ShouldExist ? "Case should exist" : "Case should not exist";
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

        /// <inheritdoc />
        internal override string ScriptText =>
            @"
    begin
        the_case = utilities.case_factory.open(pathArg)
        the_case.close()
        if expectExistsArg == 'False'
            puts 'Case Exists but was expected not to'
            exit
        end
        puts 'Case Exists as expected'
    rescue #Case does not exist
        if expectExistsArg == 'True'
            puts 'Case does not exist but was expected to'
            exit
        end
        puts 'Case does not exist, this was expected'
    end
";

        /// <inheritdoc />
        internal override string MethodName => "DoesCaseExist";

        /// <inheritdoc />
        internal override IEnumerable<(string arg, string? val, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("expectExistsArg", ShouldExist.ToString(), false);
        }
    }
}