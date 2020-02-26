using NuixClient.Search.Properties;
using Orchestration;

namespace NuixClient.Search
{
    internal abstract class PropertyValue
    {
        public abstract Result<string> TryRender(AbstractSearchProperty searchProperty);
    }
}