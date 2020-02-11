using System.Collections.Generic;

namespace NuixClient.Search
{
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
    }
}