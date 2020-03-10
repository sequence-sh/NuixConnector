using System.Text.RegularExpressions;

namespace Reductech.EDR.Connectors.Nuix.Search.SearchProperties
{
    internal class CompoundPropertySearchTerm : PropertySearchTerm
    {
        public readonly string? SubProperty;

        internal static readonly Regex CompoundPropertyRegex = new Regex(
            "(?<property>[a-z0-9-]+):(?<subProperty>(?:[a-z0-9-*]+)|(?:\"[ a-z0-9-*]+\")):",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal CompoundPropertySearchTerm(AbstractSearchProperty searchProperty, PropertyValue value, string asString, string? subProperty) 
            : base(searchProperty, value, asString)
        {
            SubProperty = subProperty;
        }

        public override string AsString => $"{SearchProperty.PropertyName}:{SubProperty}:{PropertyValueString}"; //put strings with non letter characters in quotes
    }
}