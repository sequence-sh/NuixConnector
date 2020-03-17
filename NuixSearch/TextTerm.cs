using System.Collections.Generic;
using System.Linq;

namespace Reductech.EDR.Connectors.Nuix.Search
{
    internal class TextTerm : ISearchTerm
    {
        public TextTerm(string text)
        {
            if (text.StartsWith("\"") && text.EndsWith("\""))
                text = text[1..^1];
            Text = text;
        }

        public readonly string Text;


        public string AsString
        {
            get
            {
                if (Text.Contains(" "))
                    return '"' + Text + '"'; //if text contains multiple words, put it in quotes
                else return Text;
            }
        }

        public IEnumerable<string> ErrorMessages => Enumerable.Empty<string>();

        /// <inheritdoc />
        public bool Matches(ISearchableObject searchableObject)
        {
            return searchableObject.AllPropertyValues.Any(x => x.Contains(Text));
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
    }
}