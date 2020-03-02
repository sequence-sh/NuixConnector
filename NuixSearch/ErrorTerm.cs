using System.Collections.Generic;

namespace NuixSearch
{
    internal class ErrorTerm : ISearchTerm
    {
        public readonly string ErrorString;

        public ErrorTerm(string errorString)
        {
            ErrorString = errorString;
        }

        public string AsString => ErrorString;
        public IEnumerable<string> ErrorMessages { get{ yield return ErrorString;}}
    }
}