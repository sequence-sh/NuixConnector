using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.Processes
{
    /// <summary>
    /// Migrates a case to the latest version if necessary.
    /// </summary>
    public sealed class NuixMigrateCase : RubyScriptProcess
    {
        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [YamlMember(Order = 2)]
        [ExampleValue("C:/Cases/MyCase")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => "Migrate Case";

        /// <inheritdoc />
        internal override string ScriptText => @"
    puts ""Opening Case, migrating if necessary""
    
    options = {migrate: true}

    the_case = utilities.case_factory.open(pathArg, options)";

        /// <inheritdoc />
        internal override string MethodName => "MigrateCase";

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
        }
    }
}