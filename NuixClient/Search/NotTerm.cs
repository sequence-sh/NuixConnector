using System.Collections.Generic;

namespace NuixClient.Search
{
    internal class NotTerm : ISearchTerm
    {
        public NotTerm(ISearchTerm term)
        {
            Term = term;
        }

        public readonly ISearchTerm Term;

        public string AsString
        {
            get
            {
                if (Term is TextTerm)
                {
                    return "NOT " + Term.AsString;
                }
                else
                {
                    return "NOT (" + Term.AsString + ")";
                }
            }
        }

        public override string ToString()
        {
            return AsString;
        }

        public override int GetHashCode()
        {
            return AsString.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return obj is ISearchTerm st && st.AsString == AsString;
        }

        public IEnumerable<string> ErrorMessages => Term.ErrorMessages;
    }
}