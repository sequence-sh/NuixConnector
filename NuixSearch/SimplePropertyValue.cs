using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Search.SearchProperties;

namespace Reductech.EDR.Connectors.Nuix.Search
{
    internal class SimplePropertyValue : PropertyValue
    {
        public SimplePropertyValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override Result<string> TryRender(AbstractSearchProperty searchProperty)
        {
            return searchProperty.TryRender(Value);
        }

        /// <inheritdoc />
        public override bool Matches(string s)
        {
            return Value.Equals(s);
        }

        public override string ToString()
        {
            return Value;
        }
    }
}