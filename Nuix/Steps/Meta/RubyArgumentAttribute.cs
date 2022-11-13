namespace Sequence.Connectors.Nuix.Steps.Meta;

/// <summary>
/// Indicates that this argument is to be passed to a ruby function.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class RubyArgumentAttribute : Attribute
{
    /// <inheritdoc />
    public RubyArgumentAttribute(string rubyName) => RubyName = rubyName;

    /// <summary>
    /// The name of the argument in Ruby.
    /// </summary>
    public string RubyName { get; }
}
