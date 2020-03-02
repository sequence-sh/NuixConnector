using CSharpFunctionalExtensions;

namespace NuixSearch.SearchProperties
{
    internal class RangeSearchProperty : GenericSearchProperty<Range>
    {
        public RangeSearchProperty(string propertyName) : base(propertyName)
        {
        }

        protected override Result<Range> TryParse(string s)
        {
            return Range.TryParse(s);
        }
    }
}