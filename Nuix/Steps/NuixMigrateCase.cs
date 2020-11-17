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
    /// Migrates a case to the latest version if necessary.
    /// </summary>
    public sealed class NuixMigrateCaseStepFactory : RubyScriptStepFactory<NuixMigrateCase, Unit>
    {
        private NuixMigrateCaseStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<NuixMigrateCase, Unit> Instance { get; } = new NuixMigrateCaseStepFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(7, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>();

        /// <inheritdoc />
        public override string FunctionName => "MigrateCase";

        /// <inheritdoc />
        public override string RubyFunctionText => @"
    log ""Opening Case, migrating if necessary""

    options = {migrate: true}

    the_case = $utilities.case_factory.open(pathArg, options)";
    }


    /// <summary>
    /// Migrates a case to the latest version if necessary.
    /// </summary>
    public sealed class NuixMigrateCase : RubyScriptStepBase<Unit>
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => NuixMigrateCaseStepFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [StepProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]

        public IStep<string> CasePath { get; set; } = null!;
    }
}