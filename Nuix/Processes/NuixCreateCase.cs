using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Creates a new case.
    /// </summary>
    public sealed class NuixCreateCaseProcessFactory : RubyScriptProcessFactory<NuixCreateCase, Unit>
    {
        private NuixCreateCaseProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixCreateCase, Unit> Instance { get; } = new NuixCreateCaseProcessFactory();

        /// <inheritdoc />
        public override Version RequiredVersion { get; } = new Version(2, 16);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>() { NuixFeature.CASE_CREATION };
    }

    /// <summary>
    /// Creates a new case.
    /// </summary>
    public sealed class NuixCreateCase : RubyScriptProcess
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixCreateCaseProcessFactory.Instance;

        //public override string GetName() => "Create Case";

        /// <summary>
        /// The name of the case to create.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CaseName { get; set; }

        /// <summary>
        /// The path to the folder to create the case in.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        public string CasePath { get; set; }

        /// <summary>
        /// Name of the investigator.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        public string Investigator { get; set; }

        /// <summary>
        /// Description of the case.
        /// </summary>
        [RunnableProcessProperty]
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
        internal override IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("nameArg", CaseName, false);
            yield return ("descriptionArg", Description, true);
            yield return ("investigatorArg", Investigator, false);
        }
    }
}