using Orchestration;

namespace NuixClient.Search.Properties
{
    internal sealed class BooleanSearchProperty : GenericSearchProperty<bool>
    {
        public BooleanSearchProperty(string propertyName) : base(propertyName)
        {
        }

        protected override Result<bool> TryParse(string s)
        {
            if(bool.TryParse(s, out var r))
                return Result<bool>.Success(r);
            return Result<bool>.Failure($"Could not parse '{s}' as a boolean.");
        }
    }
}