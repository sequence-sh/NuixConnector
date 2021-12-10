namespace Reductech.EDR.Connectors.Nuix.Tests.Steps.Meta;

public class
    FakeNuixStreamFunctionFactory : RubyScriptStepFactory<FakeNuixStreamFunction, StringStream>
{
    internal FakeNuixStreamFunctionFactory() { }

    public static RubyScriptStepFactory<FakeNuixStreamFunction, StringStream> Instance { get; } =
        new FakeNuixStreamFunctionFactory();

    public override Version RequiredNuixVersion { get; } = new(8, 0);

    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>();

    public override string FunctionName => "EntityStream";
    public override string RubyFunctionText => @"log messageArg";
}

public class FakeNuixStreamFunction : RubyCaseScriptStepBase<StringStream>
{
    public override IRubyScriptStepFactory<StringStream> RubyScriptStepFactory =>
        FakeNuixStreamFunctionFactory.Instance;

    [Required]
    [StepProperty(1)]
    [RubyArgument("entityStream")]
    public IStep<Array<Entity>> EntityStream { get; set; } = null!;
}

public sealed class
    FakeNuixTwoStreamFunctionFactory : RubyScriptStepFactory<FakeNuixTwoStreamFunction, Unit>
{
    internal FakeNuixTwoStreamFunctionFactory() { }

    public static FakeNuixTwoStreamFunctionFactory Instance { get; } =
        new();

    public override Version RequiredNuixVersion { get; } = new(8, 0);

    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>();

    public override string FunctionName => "TwoStreams";
    public override string RubyFunctionText => @"log 'oh no!'";
}

public class FakeNuixTwoStreamFunction : RubyCaseScriptStepBase<Unit>
{
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        FakeNuixTwoStreamFunctionFactory.Instance;

    [Required]
    [StepProperty(1)]
    [RubyArgument("stream1Arg")]
    public IStep<Array<Entity>> Stream1 { get; set; } = null!;

    [Required]
    [StepProperty(2)]
    [RubyArgument("stream2Arg")]
    public IStep<Array<Entity>> Stream2 { get; set; } = null!;
}
