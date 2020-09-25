using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;
using Reductech.EDR.Processes.Util;

namespace Reductech.EDR.Connectors.Nuix.Processes
{

    /// <summary>
    /// Migrates a case to the latest version if necessary.
    /// </summary>
    public sealed class NuixMigrateCaseProcessFactory : RubyScriptProcessFactory<NuixMigrateCase, Unit>
    {
        private NuixMigrateCaseProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixMigrateCase, Unit> Instance { get; } = new NuixMigrateCaseProcessFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion { get; } = new Version(7, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>();

        /// <inheritdoc />
        public override string FunctionName => "MigrateCase";

        /// <inheritdoc />
        public override string RubyFunctionText => @"
    puts ""Opening Case, migrating if necessary""

    options = {migrate: true}

    the_case = utilities.case_factory.open(pathArg, options)";
    }


    /// <summary>
    /// Migrates a case to the latest version if necessary.
    /// </summary>
    public sealed class NuixMigrateCase : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<Unit> RubyScriptProcessFactory => NuixMigrateCaseProcessFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessPropertyAttribute]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]

        public IRunnableProcess<string> CasePath { get; set; } = null!;
    }
}