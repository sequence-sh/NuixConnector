using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Adds a file or directory to a Nuix Case.
    /// </summary>
    public sealed class NuixAddItem : RubyScriptProcess
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => $"Add '{Path}'";

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// The path of the file or directory to add to the case.
        /// </summary>
        [Required]
        [YamlMember( Order = 3)]
        [ExampleValue("C:/Data/File.txt")]
        public string Path { get; set; }

        /// <summary>
        /// The custodian to assign to the new folder.
        /// </summary>
        [Required]
        [YamlMember(Order = 4)]
        public string Custodian { get; set; }

        /// <summary>
        /// The description of the new folder.
        /// </summary>
        [YamlMember(Order = 5)]
        public string? Description { get; set; }

        /// <summary>
        /// The name of the folder to create.
        /// </summary>
        [Required]
        [YamlMember(Order = 6)]
        public string FolderName { get; set; }

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [YamlMember(Order = 7)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }


        /// <summary>
        /// The name of the processing profile to use.
        /// </summary>
        
        [YamlMember(Order = 7)]
        [ExampleValue("MyProcessingProfile")]
        [DefaultValueExplanation("The default processing profile will be used.")]
        public string? ProcessingProfileName { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        internal override string ScriptName => "AddToCase.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-n", FolderName);
            if(Description != null)
                yield return ("-d", Description);
            yield return ("-c", Custodian);
            yield return ("-f", Path);
            if(ProcessingProfileName != null)
                yield return ("-r", ProcessingProfileName);
        }
    }
}