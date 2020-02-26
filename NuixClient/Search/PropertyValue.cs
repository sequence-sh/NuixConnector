using CSharpFunctionalExtensions;
using NuixClient.Search.Properties;

namespace NuixClient.Search
{
    internal abstract class PropertyValue
    {
        public abstract Result<string> TryRender(AbstractSearchProperty searchProperty);
    }
}