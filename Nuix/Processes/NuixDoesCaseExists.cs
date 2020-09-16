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
        public override Version RequiredNuixVersion { get; } = new Version(2, 16);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>();

        /// <inheritdoc />
        public override string FunctionName => "DoesCaseExist";

        /// <inheritdoc />
        public override string RubyFunctionText =>
            @"
    begin
        the_case = utilities.case_factory.open(pathArg)
        the_case.close()
        return true
    rescue #Case does not exist
        return false
    end
";
    }


    /// <summary>
    /// Returns whether or not a case exists.
    /// </summary>
    public sealed class NuixDoesCaseExists : RubyScriptProcessTyped<bool>
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory<bool> RubyScriptProcessFactory => NuixDoesCaseExistsProcessFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IRunnableProcess<string> CasePath { get; set; } = null!;


        /// <inheritdoc />
        public override bool TryParse(string s, out bool result) => bool.TryParse(s, out result);
    }
}