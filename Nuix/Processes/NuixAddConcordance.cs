using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Adds data from a Concordance file to a NUIX case.
    /// </summary>
    public sealed class NuixAddConcordance : RubyScriptProcess
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName()
        {
            return "Add Concordance";
        }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <summary>
        /// The name of the concordance profile to use.
        /// </summary>
        [Required]
        [YamlMember(Order = 3)]
        [ExampleValue("MyProfile")]
        public string ConcordanceProfileName { get; set; }

        /// <summary>
        /// The concordance date format to use.
        /// </summary>
        [Required]
        [YamlMember(Order = 4)]
        [ExampleValue("yyyy-MM-dd'T'HH:mm:ss.SSSZ")]
        public string ConcordanceDateFormat { get; set; }

        /// <summary>
        /// The path of the concordance file to import.
        /// </summary>
        [Required]
        [YamlMember(Order = 5)]
        [ExampleValue("C:/MyConcordance.dat")]
        public string FilePath { get; set; }

        /// <summary>
        /// The name of the custodian to assign the folder to.
        /// </summary>
        [Required]
        [YamlMember(Order = 6)]
        public string Custodian { get; set; }

        /// <summary>
        /// A description to add to the folder.
        /// </summary>
        [YamlMember(Order = 7)]
        public string? Description { get; set; }

        /// <summary>
        /// The name of the folder to create.
        /// </summary>
        [Required]
        [YamlMember(Order = 8)]
        public string FolderName { get; set; }

        /// <summary>
        /// The path to the case to import into.
        /// </summary>
        [Required]
        [YamlMember(Order = 9)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
        }

        internal override string ScriptName => "AddConcordanceToCase.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-n", FolderName);
            if(Description != null)
                yield return ("-d", Description);
            yield return ("-c", Custodian);
            yield return ("-f", FilePath);
            yield return ("-z", ConcordanceDateFormat);
            yield return ("-t", ConcordanceProfileName);
        }
    }
}