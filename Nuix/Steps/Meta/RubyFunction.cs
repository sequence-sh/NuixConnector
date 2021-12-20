using System.Linq;
using System.Text;

namespace Reductech.Sequence.Connectors.Nuix.Steps.Meta;

/// <summary>
/// A function that can be run within Ruby
/// </summary>
public record RubyFunction<T> : RubyFunction
{
    /// <summary>
    /// Create a new RubyFunction
    /// </summary>
    public RubyFunction(
        string functionName,
        string functionText,
        IReadOnlyCollection<RubyFunctionParameter> arguments,
        Version requiredNuixVersion,
        IReadOnlyCollection<NuixFeature> requiredNuixFeatures,
        IReadOnlyCollection<IRubyHelper>? requiredHelpers = null) : base(
        functionName,
        functionText,
        arguments,
        requiredNuixVersion,
        requiredNuixFeatures,
        requiredHelpers
    ) { }
}

/// <summary>
/// A function that can be run within Ruby
/// </summary>
public abstract record RubyFunction
{
    /// <summary>
    /// Create a new RubyFunction
    /// </summary>
    public RubyFunction(
        string functionName,
        string functionText,
        IReadOnlyCollection<RubyFunctionParameter> arguments,
        Version requiredNuixVersion,
        IReadOnlyCollection<NuixFeature> requiredNuixFeatures,
        IReadOnlyCollection<IRubyHelper>? requiredHelpers = null)
    {
        FunctionName         = functionName;
        FunctionText         = functionText;
        Arguments            = arguments;
        RequiredNuixVersion  = requiredNuixVersion;
        RequiredNuixFeatures = requiredNuixFeatures;
        RequiredHelpers      = requiredHelpers;
    }

    /// <summary>
    /// The name of this function. Should be unique.
    /// </summary>
    public string FunctionName { get; }

    /// <summary>
    /// The text of this function. Does not include the header.
    /// </summary>
    public string FunctionText { get; }

    /// <summary>
    /// Arguments to the function
    /// </summary>
    public IReadOnlyCollection<RubyFunctionParameter> Arguments { get; }

    /// <summary>
    /// Required nuix version.
    /// </summary>
    public Version RequiredNuixVersion { get; }

    /// <summary>
    /// Required nuix features.
    /// </summary>
    public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures { get; }

    /// <summary>
    /// Any helper functions required for this Step to execute.
    /// </summary>
    public IReadOnlyCollection<IRubyHelper>? RequiredHelpers { get; }

    /// <summary>
    /// Compiles the text of a function
    /// </summary>
    public string CompileFunctionText()
    {
        var stringBuilder = new StringBuilder();

        var methodHeader = $@"def {FunctionName}(args)";

        stringBuilder.AppendLine(methodHeader);

        foreach (var parameter in Arguments)
        {
            stringBuilder.AppendLine(
                $"{parameter.ParameterName} = args['{parameter.PropertyName}']"
            );
        }

        stringBuilder.AppendLine(IndentFunctionText(FunctionText));
        stringBuilder.AppendLine("end");
        stringBuilder.AppendLine();

        return stringBuilder.ToString();

        static string IndentFunctionText(string functionText)
        {
            functionText = functionText.Trim('\n', '\r');

            if (functionText.StartsWith('\t') || functionText.StartsWith(' '))
                return functionText;

            var indentedText = string.Join(
                Environment.NewLine,
                functionText.Split(Environment.NewLine).Select(x => '\t' + x)
            );

            return indentedText;
        }
    }
}
