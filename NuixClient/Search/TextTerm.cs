using System.Collections.Generic;
using System.Linq;

namespace NuixClient.Search
{
    internal class TextTerm : ISearchTerm
    {
        public TextTerm(string text)
        {
            if (text.StartsWith("\"") && text.EndsWith("\""))
                text = text[1..^1];

            //TODO escape quotes??

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