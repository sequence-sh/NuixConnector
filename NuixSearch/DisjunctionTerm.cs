using System.Collections.Generic;
using System.Linq;

namespace Reductech.EDR.Connectors.Nuix.Search
{
    /// <summary>
    /// A group of terms connected with 'OR'
    /// </summary>
    internal class DisjunctionTerm : ISearchTerm
    {
        public string AsString
        {
            get
            {
                var r = string.Join(" OR ", Terms.Select(Write));
                return r;

                static string Write(ISearchTerm st)
                {
                    return st is ConjunctionTerm ? $"({st.AsString})" : st.AsString;
                }
            }
        }

        public IEnumerable<string> ErrorMessages => Terms.SelectMany(x => x.ErrorMessages);

        public DisjunctionTerm(IEnumerable<ISearchTerm> terms)
        {
            Terms = terms.ToList();
        }

        public readonly IReadOnlyCollection<ISearchTerm> Terms;

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
    }
}
