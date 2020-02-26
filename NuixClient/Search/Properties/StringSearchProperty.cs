using Orchestration;

namespace NuixClient.Search.Properties
{
    internal class StringSearchProperty : GenericSearchProperty<string>
    {
        public StringSearchProperty(string propertyName) : base(propertyName)
        {
        }

        protected override Result<string> TryParse(string s)
        {
            return string.IsNullOrWhiteSpace(s)?
                Result<string>.Failure("String is null or empty") : 
                Result<string>.Success(s);
        }
    }
}