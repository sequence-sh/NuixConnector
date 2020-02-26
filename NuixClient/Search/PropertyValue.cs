using NuixClient.Search.Properties;

namespace NuixClient.Search
{
    internal abstract class PropertyValue
    {
        public abstract bool Render(AbstractSearchProperty searchProperty, out string? value);
    }
}