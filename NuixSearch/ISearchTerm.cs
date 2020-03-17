using System.Collections.Generic;
using System.Linq;

namespace Reductech.EDR.Connectors.Nuix.Search
{

    public class SearchableObject : ISearchableObject
    {
        private readonly IReadOnlyDictionary<string, string> _dictionary;

        public SearchableObject(string property, string value)
        {
            _dictionary = new Dictionary<string, string> {{property, value}};
        }

        public SearchableObject(params KeyValuePair<string, string>[] properties)
        {
            _dictionary = new Dictionary<string, string>(properties);
        }

        public static SearchableObject FromString(string s)
        {
            var properties = s.Split('|')
                .Select(x => new KeyValuePair<string, string>(
                    x.Substring(0, x.LastIndexOf(':')).Trim(), x.Substring(x.LastIndexOf(':') + 1).Trim())).ToArray();

            return new SearchableObject(properties);
        }

        /// <inheritdoc />
        public string? this[string propertyName] => _dictionary.TryGetValue(propertyName, out var v) ? v : null;

        /// <inheritdoc />
        public IEnumerable<string> AllPropertyValues => _dictionary.Values;

        
    }

    public interface ISearchableObject
    {
        string? this[string propertyName] { get; }

        IEnumerable<string> AllPropertyValues { get; }
    }


    /// <summary>
    /// A term for searching in NUIX
    /// </summary>
    public interface ISearchTerm //TODO do dates
    {
        /// <summary>
        /// The string representation of this term
        /// </summary>
        string AsString { get; }

        /// <summary>
        /// Any errors in this search term
        /// </summary>
        IEnumerable<string> ErrorMessages { get; }

        bool Matches(ISearchableObject searchableObject);
    }
}