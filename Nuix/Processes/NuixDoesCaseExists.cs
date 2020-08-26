using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Returns whether or not a case exists.
    /// </summary>
    public sealed class NuixDoesCaseExistsProcessFactory : RubyScriptProcessFactory<NuixDoesCaseExists, bool>
    {
        private NuixDoesCaseExistsProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixDoesCaseExists, bool> Instance { get; } = new NuixDoesCaseExistsProcessFactory();

        /// <inheritdoc />
        public override Version RequiredVersion { get; } = new Version(2, 16);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>();
    }


    /// <summary>
    /// Returns whether or not a case exists.
    /// </summary>
    public sealed class NuixDoesCaseExists : RubyScriptProcessTyped<bool>
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixDoesCaseExistsProcessFactory.Instance;

        ///// <inheritdoc />
        //public override string GetName()
        //{
        //    return "Does Case Exist?";
        //}

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IRunnableProcess<string> CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <inheritdoc />
        internal override string ScriptText =>
            @"
    begin
        the_case = utilities.case_factory.open(pathArg)
        the_case.close()
        return true
    rescue #Case does not exist
        return false
    end
";

        /// <inheritdoc />
        public override string MethodName => "DoesCaseExist";

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, IRunnableProcess? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
        }

        /// <inheritdoc />
        public override bool TryParse(string s, out bool result) => bool.TryParse(s, out result);
    }
}