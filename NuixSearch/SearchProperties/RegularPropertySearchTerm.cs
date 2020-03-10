using System.Text.RegularExpressions;

namespace Reductech.EDR.Connectors.Nuix.Search.SearchProperties
{
    internal class RegularPropertySearchTerm : PropertySearchTerm
    {
        internal static readonly Regex PropertyRegex = new Regex(
            "(?<property>[a-z0-9-]+):",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal RegularPropertySearchTerm(AbstractSearchProperty searchProperty, PropertyValue value, string asString) : base(searchProperty, value, asString)
        {
        }

        public override string AsString => $"{SearchProperty.PropertyName}:{PropertyValueString}";
    }
}