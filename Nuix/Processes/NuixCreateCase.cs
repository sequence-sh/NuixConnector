using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Processes
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
        public override Version RequiredNuixVersion { get; } = new Version(2, 16);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>() { NuixFeature.CASE_CREATION };


        /// <inheritdoc />
        public override string FunctionName => "CreateCase";


        /// <inheritdoc />
        public override string RubyFunctionText => @"
    puts 'Creating Case'
    the_case = utilities.case_factory.create(pathArg,
    :name => nameArg,
    :description => descriptionArg,
    :investigator => investigatorArg)
    puts 'Case Created'
    the_case.close";

    }

    /// <summary>
    /// Creates a new case.
    /// </summary>
    public sealed class NuixCreateCase : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<Unit> RubyScriptProcessFactory => NuixCreateCaseProcessFactory.Instance;

        /// <summary>
        /// The path to the folder to create the case in.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IRunnableProcess<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The name of the case to create.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [RubyArgument("nameArg", 2)]
        public IRunnableProcess<string> CaseName { get; set; }= null!;


        /// <summary>
        /// Description of the case.
        /// </summary>
        [RunnableProcessProperty]
        [RubyArgument("descriptionArg", 3)]
        public IRunnableProcess<string>? Description { get; set; }

        /// <summary>
        /// Name of the investigator.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [RubyArgument("investigatorArg", 4)]
        public IRunnableProcess<string> Investigator { get; set; }= null!;
    }
}