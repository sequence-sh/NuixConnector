using System.Collections.Generic;
using System.Linq;

namespace NuixClient.Search.Properties
{
    internal class ComplexPropertyValue : PropertyValue
    {
        public ComplexPropertyValue(IEnumerable<SimplePropertyValue> disjunction)
        {
            Disjunction = disjunction.ToList();
        }

        public IReadOnlyList<SimplePropertyValue> Disjunction { get; }

        public override bool Render(AbstractSearchProperty searchProperty, out string? value)
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
}