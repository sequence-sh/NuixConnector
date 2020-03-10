using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Search.SearchProperties;

namespace Reductech.EDR.Connectors.Nuix.Search
{
    internal abstract class PropertyValue
    {
        public abstract Result<string> TryRender(AbstractSearchProperty searchProperty);
    }
}