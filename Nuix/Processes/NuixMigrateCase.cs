using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes
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
        public override Version RequiredVersion { get; } = new Version(3, 0);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>();
    }


    /// <summary>
    /// Migrates a case to the latest version if necessary.
    /// </summary>
    public sealed class NuixMigrateCase : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixMigrateCaseProcessFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IRunnableProcess<string>  CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        ///// <inheritdoc />
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public override string GetName() => "Migrate Case";

        /// <inheritdoc />
        internal override string ScriptText => @"
    puts ""Opening Case, migrating if necessary""

    options = {migrate: true}

    the_case = utilities.case_factory.open(pathArg, options)";

        /// <inheritdoc />
        public override string MethodName => "MigrateCase";

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, IRunnableProcess? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
        }
    }
}