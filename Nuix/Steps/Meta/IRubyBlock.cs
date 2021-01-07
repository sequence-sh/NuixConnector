using System;
using System.Collections.Generic;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{

/// <summary>
/// A function that can be run within Ruby
/// </summary>
public interface IRubyFunction
{
    /// <summary>
    /// The name of this function. Should be unique.
    /// </summary>
    string FunctionName { get; }

    /// <summary>
    /// The text of this function. Does not include the header.
    /// </summary>
    string FunctionText { get; }

    /// <summary>
    /// Arguments to the function
    /// </summary>
    IReadOnlyCollection<RubyFunctionParameter> Arguments { get; }

    /// <summary>
    /// Required nuix version.
    /// </summary>
    public Version RequiredNuixVersion { get; }

    /// <summary>
    /// Required nuix features.
    /// </summary>
    public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures { get; }
}

/// <summary>
/// A ruby function that returns a particular type or a unit.
/// </summary>
public interface IRubyFunction<T> : IRubyFunction { }

/// <summary>
/// A ruby function that returns a particular type or a unit.
/// </summary>
public class RubyFunction<T> : IRubyFunction<T>
{
    /// <summary>
    /// Create a new RubyFunction
    /// </summary>
    public RubyFunction(
        string functionName,
        string functionText,
        IReadOnlyCollection<RubyFunctionParameter> arguments)
    {
        FunctionName = functionName;
        FunctionText = functionText;
        Arguments    = arguments;
    }

    /// <inheritdoc />
    public string FunctionName { get; }

    /// <inheritdoc />
    public string FunctionText { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<RubyFunctionParameter> Arguments { get; }

    /// <inheritdoc />
    public Version RequiredNuixVersion { get; set; } = NuixVersionHelper.DefaultRequiredVersion;

    /// <inheritdoc />
    public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures { get; set; } =
        new List<NuixFeature>();
}

}
