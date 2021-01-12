using System;
using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{

/// <summary>
/// Migrates a case to the latest version if necessary.
/// </summary>
public sealed class NuixCloseCaseStepFactory : RubyScriptStepFactory<NuixCloseCase, Unit>
{
    private NuixCloseCaseStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixCloseCase, Unit> Instance { get; } =
        new NuixCloseCaseStepFactory();

    /// <inheritdoc />
    public override Version RequiredNuixVersion { get; } = new(7, 2);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>();

    /// <inheritdoc />
    public override string FunctionName => "OpenCase";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    log ""Closing Case""
    currentCase.close()
    log ""Case Closed""";
}

/// <summary>
/// Migrates a case to the latest version if necessary.
/// </summary>
public sealed class NuixCloseCase : RubyScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixCloseCaseStepFactory.Instance;
}

}
