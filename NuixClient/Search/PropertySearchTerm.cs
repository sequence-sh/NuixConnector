using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace NuixClient.Search
{
    internal class ComplexPropertyValue : PropertyValue
    {
        public ComplexPropertyValue(IEnumerable<SimplePropertyValue> disjunction)
        {
            Disjunction = disjunction.ToList();
        }

        public IReadOnlyList<SimplePropertyValue> Disjunction { get; }

        public override bool Render(SearchProperty searchProperty, out string? value)
        {
            var vs = new List<string>();

            foreach (var simplePropertyValue in Disjunction)
            {
                if (simplePropertyValue.Render(searchProperty, out var v))
                {
                    vs.Add(v);
                }
                else
                {
                    value = null;
                    return false;
                }
            }

            value = @$"({string.Join(" OR ", vs.Select(x => x))})";

            return vs.Any();
        }

        public override string ToString()
        {
            return @$"({string.Join(" OR ", Disjunction.Select(x => x.ToString()))})";
        }
    }


    internal class SimplePropertyValue : PropertyValue
    {
        public SimplePropertyValue(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override bool Render(SearchProperty searchProperty, out string? value)
        {
            return searchProperty.RenderValue(Value, out value);
        }

        public override string ToString()
        {
            return Value;
        }
    }

    internal abstract class PropertyValue
    {
        public abstract bool Render(SearchProperty searchProperty, out string? value);
    }



    internal class RegularPropertySearchTerm : PropertySearchTerm
    {
        internal static readonly Regex PropertyRegex = new Regex(
            "(?<property>[a-z0-9-]+):",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal RegularPropertySearchTerm(SearchProperty searchProperty, PropertyValue value, string asString) : base(searchProperty, value, asString)
        {
        }

        public override string AsString => $"{SearchProperty.PropertyName}:{PropertyValueString}";
    }

    internal class CompoundPropertySearchTerm : PropertySearchTerm
    {
        public readonly string? SubProperty;

        internal static readonly Regex CompoundPropertyRegex = new Regex(
            "(?<property>[a-z0-9-]+):(?<subProperty>(?:[a-z0-9-*]+)|(?:\"[ a-z0-9-*]+\")):",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        internal CompoundPropertySearchTerm(SearchProperty searchProperty, PropertyValue value, string asString, string? subProperty) 
            : base(searchProperty, value, asString)
        {
            SubProperty = subProperty;
        }

        public override string AsString => $"{SearchProperty.PropertyName}:{SubProperty}:{PropertyValueString}"; //put strings with non letter characters in quotes
    }


    internal abstract class PropertySearchTerm : ISearchTerm
    {
        public static ISearchTerm TryCreate(string propertyString, PropertyValue value)
        { 
            string propertyName;
            string? subProperty;

            if (CompoundPropertySearchTerm.CompoundPropertyRegex.TryMatch(propertyString, out var compoundMatch))
            {
                propertyName = compoundMatch.Groups["property"].Value;
                subProperty = compoundMatch.Groups["subProperty"].Value;
            }
            else if(RegularPropertySearchTerm.PropertyRegex.TryMatch(propertyString, out var pMatch))
            {
                propertyName = pMatch.Groups["property"].Value;
                subProperty = null;
            }
            else
                return new ErrorTerm($"'{propertyString}' could not be parsed as a property");

            if (!AllProperties.TryGetValue(propertyName, out var sp))
                return new ErrorTerm($"'{propertyString}' is not a recognized property");

            var s = value.Render(sp, out var propertyValueString);

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse - for compiler
            if (s && propertyValueString != null)
                return subProperty == null
                    ? new RegularPropertySearchTerm(sp, value, propertyValueString) as PropertySearchTerm
                    : new CompoundPropertySearchTerm(sp, value, propertyValueString, subProperty);

            return new ErrorTerm($"'{value}' is an invalid value for '{propertyString}'");

        }

        protected PropertySearchTerm(SearchProperty searchProperty, PropertyValue value, string propertyValueString)
        {
            SearchProperty = searchProperty;
            PropertyValue = value;

            PropertyValueString = propertyValueString;
        }

        public readonly SearchProperty SearchProperty;
        
        public readonly PropertyValue PropertyValue;

        protected string PropertyValueString { get; }

        public abstract string AsString { get; }
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
            new List<SearchProperty>
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
                new SearchProperty<string>("document-id", s=>(!string.IsNullOrWhiteSpace(s), s)),
                new SearchProperty<string>("flag", s=>(!string.IsNullOrWhiteSpace(s), s)),
#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
                
                //TODO other properties
            }.ToDictionary(x => x.PropertyName, StringComparer.OrdinalIgnoreCase);
    }
}