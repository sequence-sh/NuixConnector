using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Search.SearchProperties;

namespace Reductech.EDR.Connectors.Nuix.Search
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
                if (Term is TextTerm || Term is PropertySearchTerm)
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