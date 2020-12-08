using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps.Meta
{
    public class FakeNuixStreamFunctionFactory : RubyScriptStepFactory<FakeNuixStreamFunction, string>
    {
        internal FakeNuixStreamFunctionFactory() { }
        public static RubyScriptStepFactory<FakeNuixStreamFunction, string> Instance { get; } = new FakeNuixStreamFunctionFactory();
        public override Version RequiredNuixVersion { get; } = new Version(8, 0);
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>();
        public override string FunctionName => "EntityStream";
        public override string RubyFunctionText => @"log messageArg";
    }

    public class FakeNuixStreamFunction : RubyScriptStepBase<string>
    {
        public override IRubyScriptStepFactory<string> RubyScriptStepFactory => FakeNuixStreamFunctionFactory.Instance;

        [Required]
        [StepProperty]
        [RubyArgument("entityStream", 1)]
        public IStep<EntityStream> EntityStream { get; set; } = null!;
    }

    public sealed class FakeNuixTwoStreamFunctionFactory : RubyScriptStepFactory<FakeNuixTwoStreamFunction, Unit>
    {
        internal FakeNuixTwoStreamFunctionFactory() { }
        public static FakeNuixTwoStreamFunctionFactory Instance { get; } = new FakeNuixTwoStreamFunctionFactory();
        public override Version RequiredNuixVersion { get; } = new Version(8, 0);
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>();
        public override string FunctionName => "TwoStreams";
        public override string RubyFunctionText => @"log 'oh no!'";
    }

    public class FakeNuixTwoStreamFunction : RubyScriptStepBase<Unit>
    {
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => FakeNuixTwoStreamFunctionFactory.Instance;

        [Required]
        [StepProperty]
        [RubyArgument("stream1Arg", 1)]
        public IStep<EntityStream> Stream1 { get; set; } = null!;

        [Required]
        [StepProperty]
        [RubyArgument("stream2Arg", 2)]
        public IStep<EntityStream> Stream2 { get; set; } = null!;
    }

}