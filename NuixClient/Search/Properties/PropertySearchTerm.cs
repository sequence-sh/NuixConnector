using System;
using System.Collections.Generic;
using System.Linq;

namespace NuixClient.Search.Properties
{
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

        protected PropertySearchTerm(AbstractSearchProperty searchProperty, PropertyValue value, string propertyValueString)
        {
            SearchProperty = searchProperty;
            PropertyValue = value;

            PropertyValueString = propertyValueString;
        }

        public readonly AbstractSearchProperty SearchProperty;
        
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
        //TODO keep track of which of these take subProperties and the valid values for those 
        public static IReadOnlyDictionary<string, AbstractSearchProperty> AllProperties = 
            new List<AbstractSearchProperty>
            {
                new BooleanSearchProperty("has-custodian"),
                new BooleanSearchProperty("has-embedded-data"),
                new BooleanSearchProperty("has-text"),
                new BooleanSearchProperty("has-image"),

                new RangeSearchProperty("date-properties"),
                new RangeSearchProperty("file-size"),
                new RangeSearchProperty("recipient-count"),

                new FileTypeSearchProperty("mime-type"),

                new StringSearchProperty("tag"),
                new StringSearchProperty("kind"),
                new StringSearchProperty("comment"),
                new StringSearchProperty("from-mail-domain"),
                new StringSearchProperty("to-mail-domain"),
                new StringSearchProperty("cc-mail-domain"),
                new StringSearchProperty("bcc-mail-domain"),
                new StringSearchProperty("custodian1"),
                new StringSearchProperty("document-id"),
                new StringSearchProperty("flag"),


                new GuidSearchProperty("item-set-guid"),
                new GuidSearchProperty("production-set-guid"),
                
                //TODO other properties
            }.ToDictionary(x => x.PropertyName, StringComparer.OrdinalIgnoreCase);
    }
}