using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace NuixClient.Search
{
    internal static class RegexExtensions
    {
        [ContractAnnotation("=>true,match:notNull; =>false,match:null")]
        public static bool TryMatch(this Regex r, string input, out Match match)
        {
            match = r.Match(input);

            return match.Success;
        }
    }
 
    internal class PropertySearchTerm : ISearchTerm
    {
        internal static readonly Regex CompoundPropertyRegex = new Regex(
            "(?<property>[a-z0-9-]+):(?<subProperty>(?:[a-z0-9-*]+)|(?:\"[ a-z0-9-*]+\")):",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal static readonly Regex PropertyRegex = new Regex(
            "(?<property>[a-z0-9-]+):",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);


        public static ISearchTerm TryCreate(string propertyString, string valueString)
        { 
            string propertyName;
            string? subProperty;

            if (CompoundPropertyRegex.TryMatch(propertyString, out var compoundMatch))
            {
                propertyName = compoundMatch.Groups["property"].Value;
                subProperty = compoundMatch.Groups["subProperty"].Value;
            }
            else if(PropertyRegex.TryMatch(propertyString, out var pMatch))
            {
                propertyName = pMatch.Groups["property"].Value;
                subProperty = null;
            }
            else
            {
                return new ErrorTerm($"'{propertyString}' could not be parsed as a property");
            }

            if (AllProperties.TryGetValue(propertyName, out var sp))
            {
                var s = sp.Render(valueString, subProperty, out var asString);

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse - for compiler
                if (s && asString != null)
                    return new PropertySearchTerm(sp, subProperty, valueString, asString);
                else return new ErrorTerm($"'{valueString}' is an invalid value for '{propertyString}'");
            }

            return new ErrorTerm( $"'{propertyString}' is not a recognized property");
        }

        private PropertySearchTerm(SearchProperty searchProperty, string? subProperty, string valueString, string asString)
        {
            SearchProperty = searchProperty;
            ValueString = valueString;
            SubProperty = subProperty;

            AsString = asString;
        }

        public readonly SearchProperty SearchProperty;
        public readonly string? SubProperty;
        public readonly string ValueString;


        public string AsString { get; }
        public IEnumerable<string> ErrorMessages => Enumerable.Empty<string>();

        public override string ToString()
        {
            return AsString;
        }

        public override int GetHashCode()
        {
            return AsString.GetHashCode();
        }


        /// <summary>
        /// Do these search terms equal one another
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object? obj)
        {
            return obj is ISearchTerm st && st.AsString == AsString;
        }

        /// <summary>
        /// All possible search properties that one could search by
        /// </summary>
        public static IReadOnlyDictionary<string, SearchProperty> AllProperties = //TODO keep track of which of these take subProperties and the valid values for those 
            new List<SearchProperty>()
            {
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.
                new SearchProperty<Range>("date-properties", s => (Range.TryParse(s, out var r), r) ),
                new SearchProperty<Range>("file-size", s=> (Range.TryParse(s, out var r), r)),
                new SearchProperty<FileType>("mime-type", s=> (FileType.TryParse(s, out var r), r)),
                new SearchProperty<string>("tag", s=>(!string.IsNullOrWhiteSpace(s), s)),
                new SearchProperty<string>("comment", s=>(!string.IsNullOrWhiteSpace(s), s)),
                new SearchProperty<string>("from-mail-domain", s=>(!string.IsNullOrWhiteSpace(s), s)),
                new SearchProperty<string>("to-mail-domain", s=>(!string.IsNullOrWhiteSpace(s), s)),
                new SearchProperty<string>("cc-mail-domain", s=>(!string.IsNullOrWhiteSpace(s), s)),
                new SearchProperty<string>("bcc-mail-domain", s=>(!string.IsNullOrWhiteSpace(s), s)),
                new SearchProperty<Range>("recipient-count", s=> (Range.TryParse(s, out var r), r)),
                new SearchProperty<bool>("has-custodian", s=> (bool.TryParse(s, out var b), b)),
                new SearchProperty<string>("custodian1", s=>(!string.IsNullOrWhiteSpace(s), s)),
                new SearchProperty<Guid>("item-set-guid", s=> (Guid.TryParse(s, out var g), g)),
                new SearchProperty<Guid>("production-set-guid", s=> (Guid.TryParse(s, out var g), g)),
                new SearchProperty<string>("document-id", s=>(!string.IsNullOrWhiteSpace(s), s))
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
                
                //TODO other properties
            }.ToDictionary(x => x.PropertyName, StringComparer.OrdinalIgnoreCase);
    }
}