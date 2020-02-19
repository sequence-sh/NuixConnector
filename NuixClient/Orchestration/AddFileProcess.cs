﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// Adds a file or folder to a Nuix Case
    /// </summary>
    internal class AddFileProcess : RubyScriptProcess
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName() => $"Add '{FilePath}'";



#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// The path of the file or folder to add to the case
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 3)]
        public string FilePath { get; set; }

        /// <summary>
        /// The custodian to assign to the new folder
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 4)]
        public string Custodian { get; set; }

        /// <summary>
        /// The description of the new folder
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 5)]
        public string Description { get; set; }

        /// <summary>
        /// The name of the folder to create
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 6)]
        public string FolderName { get; set; }

        /// <summary>
        /// The path to the case
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 7)]
        public string CasePath { get; set; }


        /// <summary>
        /// The name of the processing profile to use - can be null
        /// </summary>
        [DataMember]
        [YamlMember(Order = 7)]
        public string? ProcessingProfileName { get; set; }


#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        internal override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
        }

        internal override string ScriptName => "AddToCase.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-n", FolderName);
            yield return ("-d", Description);
            yield return ("-c", Custodian);
            yield return ("-f", FilePath);
            if(ProcessingProfileName != null)
                yield return ("-r", ProcessingProfileName);
        }
    }
}