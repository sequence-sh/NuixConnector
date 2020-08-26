using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.enums;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Checks the print preview state of the production set.
    /// </summary>
    public sealed class NuixAssertPrintPreviewStateProcessFactory : RubyScriptProcessFactory<NuixAssertPrintPreviewState, Unit>
    {
        private NuixAssertPrintPreviewStateProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixAssertPrintPreviewState, Unit> Instance { get; } = new NuixAssertPrintPreviewStateProcessFactory();

        /// <inheritdoc />
        public override Version RequiredVersion { get; } = new Version(5, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>()
        {
            NuixFeature.PRODUCTION_SET, NuixFeature.ANALYSIS
        };

    }


    /// <summary>
    /// Checks the print preview state of the production set.
    /// </summary>
    public sealed class NuixAssertPrintPreviewState : RubyScriptProcessUnit
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixAssertPrintPreviewStateProcessFactory.Instance;


        ///// <inheritdoc />
        //public override string GetName()
        //{
        //    return $"Assert preview state is {ExpectedState}";
        //}

        /// <summary>
        /// The expected print preview state of the production set;
        /// </summary>
        [RunnableProcessProperty]
        [DefaultValueExplanation(nameof(PrintPreviewState.All))]

        public IRunnableProcess<PrintPreviewState> ExpectedState { get; set; } = new Constant<PrintPreviewState>(PrintPreviewState.All);

        /// <summary>
        /// The production set to reorder.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IRunnableProcess<string> ProductionSetName { get; set; }


        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
        public IRunnableProcess<string> CasePath { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        internal override string ScriptText =>
        @"
    the_case = utilities.case_factory.open(pathArg)
    productionSet = the_case.findProductionSetByName(productionSetNameArg)

    if(productionSet == nil)
        puts 'Production Set Not Found'
        the_case.close
        exit
    else
        r = productionSet.getPrintPreviewState()
        the_case.close

        if r == expectedStateArg
            puts ""Print preview state was #{r}, as expected.""
        else
            puts ""Print preview state was #{r}, but expected #{expectedStateArg}""
            exit
        end
    end";

        /// <inheritdoc />
        public override string MethodName => "GetPrintPreviewState";


        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, IRunnableProcess? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("productionSetNameArg", ProductionSetName, false);
            yield return ("expectedStateArg", ExpectedState, false);
        }
    }
}