using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Returns whether or not a case exists.
    /// </summary>
    public sealed class NuixDoesCaseExistsStepFactory : RubyScriptStepFactory<NuixDoesCaseExists, bool>
    {
        private NuixDoesCaseExistsStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<NuixDoesCaseExists, bool> Instance { get; } = new NuixDoesCaseExistsStepFactory();

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
    public sealed class NuixDoesCaseExists : RubyScriptStepTyped<bool>
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<bool> RubyScriptStepFactory => NuixDoesCaseExistsStepFactory.Instance;

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [StepProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IStep<string> CasePath { get; set; } = null!;


        /// <inheritdoc />
        public override bool TryParse(string s, out bool result) => bool.TryParse(s, out result);
    }
}