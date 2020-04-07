using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Creates a new case.
    /// </summary>
    internal class NuixCreateCase : RubyScriptProcess
    {
        /// <inheritdoc />
        protected override NuixReturnType ReturnType => NuixReturnType.Unit;

        public override string GetName() => "Create Case";

        /// <summary>
        /// The name of the case to create.
        /// </summary>
        [Required]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CaseName { get; set; }

        /// <summary>
        /// The path to the folder to create the case in.
        /// </summary>
        [Required]
        [YamlMember(Order = 4)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }

        /// <summary>
        /// Name of the investigator.
        /// </summary>
        [Required]
        [YamlMember(Order = 5)]
        public string Investigator { get; set; }

        /// <summary>
        /// Description of the case.
        /// </summary>
        
        [YamlMember(Order = 6)]
        public string? Description { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.




        /// <inheritdoc />
        internal override string ScriptText => @"
    puts 'Creating Case'    
    the_case = utilities.case_factory.create(pathArg,
    :name => nameArg,
    :description => descriptionArg,
    :investigator => investigatorArg)
    puts 'Case Created'
    the_case.close";

        /// <inheritdoc />
        internal override string MethodName => "CreateCase";

        /// <inheritdoc />
        internal override Version RequiredVersion { get; } = new Version(2,16);

        /// <inheritdoc />
        internal override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>(){NuixFeature.CASE_CREATION};

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("nameArg", CaseName, false);
            yield return ("descriptionArg", Description, true);
            yield return ("investigatorArg", Investigator, false);
        }
    }
}