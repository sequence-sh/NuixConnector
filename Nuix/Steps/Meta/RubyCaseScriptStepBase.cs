using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{

/// <summary>
/// A ruby script step that uses a case.
/// </summary>
public abstract class RubyCaseScriptStepBase<T> : RubyScriptStepBase<T>
{
    /// <summary>
    /// The pathArg argument name in Ruby.
    /// </summary>
    public const string PathArg = "pathArg";

    // ReSharper disable once StaticMemberInGenericType
    private static readonly RubyFunctionParameter PathParameter =
        new(PathArg, nameof(CasePath), true, null);

    /// <summary>
    /// The case path parameter
    /// </summary>
    // ReSharper disable once StaticMemberInGenericType
    public static readonly CasePathParameter StaticCasePathParameter =
        new CasePathParameter.UsesCase(PathParameter);

    /// <inheritdoc />
    public override CasePathParameter CasePathParameter => StaticCasePathParameter;

    /// <summary>
    /// The case path to use. If this is set, that case will be opened.
    /// If it is not set, the existing case will be used.
    /// If no existing case is open this will result in an error.
    /// </summary>
    [StepProperty]
    [Example("C:/Cases/MyCase")]
    [RubyArgument(PathArg)]
    [Alias("Directory")]
    [DefaultValueExplanation("Use the current open case")]
    public IStep<StringStream>? CasePath { get; set; }
}

}
