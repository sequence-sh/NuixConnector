using System.Collections.Generic;
using System.Linq;
using Orchestration;

namespace NuixClient.Search.Properties
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
            var vs = new List<string>();
            var errors = new List<string>();

            foreach (var simplePropertyValue in Disjunction)
            {
                var r = simplePropertyValue.TryRender(searchProperty);

                if (r is Success<string> s)
                    vs.Add(s.Result);
                else
                    errors.AddRange(r.Errors);
            }

            if(errors.Any())
                return Result<string>.Failure(errors);

            if(!vs.Any())
                return Result<string>.Failure($"'{searchProperty.PropertyName}' Must have at least one property value");

            var resultString = @$"({string.Join(" OR ", vs.Select(x => x))})";

            return Result<string>.Success(resultString);
        }

        public override string ToString()
        {
            return @$"({string.Join(" OR ", Disjunction.Select(x => x.ToString()))})";
        }
    }
}