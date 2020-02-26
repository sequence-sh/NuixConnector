using CSharpFunctionalExtensions;

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
                Result.Failure<string>("String is null or empty") : 
                Result.Success(s);
        }

        protected override string ConvertToString(string s)
        {
            return s.Contains(" ") ? $"\"{s}\"" : s;
        }
    }
}