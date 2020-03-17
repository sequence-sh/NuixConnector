using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;

namespace Reductech.EDR.Connectors.Nuix.Search.SearchProperties
{
    internal class ComplexPropertyValue : PropertyValue
    {
        public ComplexPropertyValue(IEnumerable<SimplePropertyValue> disjunction)
        {
            Disjunction = disjunction.ToList();
        }

        public IReadOnlyList<SimplePropertyValue> Disjunction { get; }

        public override Result<string> TryRender(AbstractSearchProperty searchProperty)
        {
            if(Disjunction.Count == 0)
                return Result.Failure<string>($"'{searchProperty.PropertyName}' Must have at least one property value");

            var result = Disjunction.Select(x => x.TryRender(searchProperty))
                .Combine()
                .Map(vs => @$"({string.Join(" OR ", vs.Select(x => x))})");

            return result;
        }

        /// <inheritdoc />
        public override bool Matches(string s)
        {
            return Disjunction.Any(x => x.Matches(s));
        }

        public override string ToString()
        {
            return @$"({string.Join(" OR ", Disjunction.Select(x => x.ToString()))})";
        }
    }
}