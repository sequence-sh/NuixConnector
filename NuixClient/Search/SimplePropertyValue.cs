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

        public override bool Render(AbstractSearchProperty searchProperty, out string? value)
        {
            return searchProperty.RenderValue(Value, out value);
        }

        public override string ToString()
        {
            return Value;
        }
    }
}