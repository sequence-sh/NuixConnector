using CSharpFunctionalExtensions;
using NuixSearch.SearchProperties;

namespace NuixSearch
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

        public override string ToString()
        {
            return Value;
        }
    }
}