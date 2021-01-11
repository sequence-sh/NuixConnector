using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps.Meta
{

public class
    FakeNuixStreamFunctionFactory : RubyScriptStepFactory<FakeNuixStreamFunction, StringStream>
{
    internal FakeNuixStreamFunctionFactory() { }

    public static RubyScriptStepFactory<FakeNuixStreamFunction, StringStream> Instance { get; } =
        new FakeNuixStreamFunctionFactory();

    public override Version RequiredNuixVersion { get; } = new Version(8, 0);

    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>();

    public override string FunctionName => "EntityStream";
    public override string RubyFunctionText => @"log messageArg";
}

public class FakeNuixStreamFunction : RubyScriptStepBase<StringStream>
{
    public override IRubyScriptStepFactory<StringStream> RubyScriptStepFactory =>
        FakeNuixStreamFunctionFactory.Instance;

    [Required]
    [StepProperty(1)]
    [RubyArgument("entityStream", 1)]
    public IStep<Array<Entity>> EntityStream { get; set; } = null!;
}

public sealed class
    FakeNuixTwoStreamFunctionFactory : RubyScriptStepFactory<FakeNuixTwoStreamFunction, Unit>
{
    internal FakeNuixTwoStreamFunctionFactory() { }

    public static FakeNuixTwoStreamFunctionFactory Instance { get; } =
        new FakeNuixTwoStreamFunctionFactory();

    public override Version RequiredNuixVersion { get; } = new Version(8, 0);

    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>();

    public override string FunctionName => "TwoStreams";
    public override string RubyFunctionText => @"log 'oh no!'";
}

public class FakeNuixTwoStreamFunction : RubyScriptStepBase<Unit>
{
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        FakeNuixTwoStreamFunctionFactory.Instance;

    [Required]
    [StepProperty(1)]
    [RubyArgument("stream1Arg", 1)]
    public IStep<Array<Entity>> Stream1 { get; set; } = null!;

    [Required]
    [StepProperty(2)]
    [RubyArgument("stream2Arg", 2)]
    public IStep<Array<Entity>> Stream2 { get; set; } = null!;
}

}
