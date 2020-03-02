using CSharpFunctionalExtensions;

namespace NuixSearch.SearchProperties
{
    internal sealed class BooleanSearchProperty : GenericSearchProperty<bool>
    {
        public BooleanSearchProperty(string propertyName) : base(propertyName)
        {
        }

        protected override Result<bool> TryParse(string s)
        {
            if(bool.TryParse(s, out var r))
                return Result.Success(r);
            if (int.TryParse(s, out var i) && (i == 1 || i == 0 ))
                return Result.Success(i == 1);
            return Result.Failure<bool>($"Could not parse '{s}' as a boolean.");
        }

        protected override string ConvertToString(bool t)
        {
            return t ? "1" : "0";
        }
    }
}