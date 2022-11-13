using CSharpFunctionalExtensions;

namespace Sequence.Connectors.Nuix.Steps.Meta;

/// <summary>
/// The casePath parameter
/// </summary>
public record CasePathParameter
{
    private CasePathParameter() { }

    /// <summary>
    /// Ignores the currently open case
    /// </summary>
    public record IgnoresOpenCase : CasePathParameter
    {
        private IgnoresOpenCase() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static IgnoresOpenCase Instance { get; } = new();
    }

    /// <summary>
    /// This function changes the currently open case
    /// </summary>
    public record ChangesOpenCase
        (Maybe<RubyFunctionParameter> NewCaseParameter) : CasePathParameter
    {
        /// <summary>
        /// The parameter pointing to the new case. If empty, the new case will be empty.
        /// </summary>
        public Maybe<RubyFunctionParameter> NewCaseParameter { get; init; } = NewCaseParameter;
    }

    /// <summary>
    /// This function uses a case
    /// </summary>
    public record UsesCase(RubyFunctionParameter CaseParameter) : CasePathParameter
    {
        /// <summary>
        /// The parameter pointing to the case to use.
        /// </summary>
        public RubyFunctionParameter CaseParameter { get; init; } = CaseParameter;
    }
}
