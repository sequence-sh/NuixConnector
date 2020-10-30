using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Creates a new case.
    /// </summary>
    public sealed class NuixCreateCaseStepFactory : RubyScriptStepFactory<NuixCreateCase, Unit>
    {
        private NuixCreateCaseStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<NuixCreateCase, Unit> Instance { get; } = new NuixCreateCaseStepFactory();

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
    public sealed class NuixCreateCase : RubyScriptStepUnit
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => NuixCreateCaseStepFactory.Instance;

        /// <summary>
        /// The path to the folder to create the case in.
        /// </summary>
        [Required]
        [StepProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IStep<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The name of the case to create.
        /// </summary>
        [Required]
        [StepProperty]
        [RubyArgument("nameArg", 2)]
        public IStep<string> CaseName { get; set; }= null!;


        /// <summary>
        /// Description of the case.
        /// </summary>
        [StepProperty]
        [RubyArgument("descriptionArg", 3)]
        [DefaultValueExplanation("No Description")]
        public IStep<string>? Description { get; set; }

        /// <summary>
        /// Name of the investigator.
        /// </summary>
        [Required]
        [StepProperty]
        [RubyArgument("investigatorArg", 4)]
        public IStep<string> Investigator { get; set; }= null!;
    }
}