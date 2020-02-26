using CSharpFunctionalExtensions;
using NuixClient.Search.Properties;

namespace NuixClient.Search
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