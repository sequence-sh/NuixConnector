using CSharpFunctionalExtensions;
using NuixSearch.SearchProperties;

namespace NuixSearch
{
    internal abstract class PropertyValue
    {
        public abstract Result<string> TryRender(AbstractSearchProperty searchProperty);
    }
}