﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process which creates a new case
    /// </summary>
    internal class CreateCaseProcess : RubyScriptProcess
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName() => $"Create Case '{CaseName}'";


        /// <summary>
        /// The name of the case to create
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CaseName { get; set; }


        /// <summary>
        /// The path to the folder to create the case in
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 4)]
        public string CasePath { get; set; }

        /// <summary>
        /// Name of the investigator
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 5)]
        public string Investigator { get; set; }

        /// <summary>
        /// Description of the case
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 6)]
        public string Description { get; set; }


        public override bool Equals(object? obj)
        {
            var r = obj is CreateCaseProcess ccp && (Conditions ?? Enumerable.Empty<Condition>()).SequenceEqual(ccp.Conditions ?? Enumerable.Empty<Condition>())
                                                     && CaseName == ccp.CaseName
                                                     && Description == ccp.Description
                                                     && Investigator == ccp.Investigator 
                                                     && CasePath == ccp.CasePath;

            return r;
        }

        public override int GetHashCode()
        {
            return GetName().GetHashCode();
        }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        internal override IEnumerable<string> GetArgumentErrors()
        {
            yield break;
        }

        internal override string ScriptName => "CreateCase.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-n", CaseName);
            yield return ("-d", Description);
            yield return ("-i", Investigator);
        }
    }
}