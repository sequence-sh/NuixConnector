using System.Text;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{

/// <summary>
/// A string builder that can accept an indentation level.
/// </summary>
public interface IIndentationStringBuilder
{
    /// <summary>
    /// Indent one level.
    /// </summary>
    public IIndentationStringBuilder Indent();

    /// <summary>
    /// Append a line to the string builder.
    /// </summary>
    public void AppendLine(string s);
}

/// <summary>
/// Does not actually do anything with the string.
/// </summary>
public class NullIndentationStringBuilder : IIndentationStringBuilder
{
    private NullIndentationStringBuilder() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static IIndentationStringBuilder Instance { get; } = new NullIndentationStringBuilder();

    /// <inheritdoc />
    public IIndentationStringBuilder Indent() => Instance;

    /// <inheritdoc />
    public void AppendLine(string s)
    {
        //Do nothing.
    }
}

/// <summary>
/// A string builder that can accept an indentation level.
/// </summary>
public class IndentationStringBuilder : IIndentationStringBuilder
{
    /// <summary>
    /// Creates a new IndentationStringBuilder.
    /// </summary>
    public IndentationStringBuilder(StringBuilder stringBuilder, int indentation)
    {
        Indentation       = indentation;
        StringBuilder     = stringBuilder;
        IndentationString = new string('\t', Indentation);
    }

    /// <summary>
    /// The string builder to append to.
    /// </summary>
    public StringBuilder StringBuilder { get; }

    /// <summary>
    /// The level of indentation.
    /// </summary>
    public int Indentation { get; }

    /// <summary>
    /// The indentation to apply.
    /// </summary>
    public string IndentationString { get; }

    /// <summary>
    /// Indent one level.
    /// </summary>
    public IIndentationStringBuilder Indent() =>
        new IndentationStringBuilder(StringBuilder, Indentation + 1);

    /// <summary>
    /// Append a line to the string builder.
    /// </summary>
    public void AppendLine(string s)
    {
        StringBuilder.AppendLine($"{IndentationString}{s}");
    }
}

}
