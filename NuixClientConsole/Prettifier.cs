using System.Collections.Generic;
using System.Linq;

namespace NuixClientConsole
{
    internal static class Prettifier
    {
        internal static IEnumerable<string> ArrangeIntoColumns(IReadOnlyCollection<IReadOnlyCollection<string?>> rows)
        {
            var columnWidthDictionary  =
                rows.SelectMany(row => row.Select((cell, i) => (cell?.Length ?? 0, i)))
                .GroupBy(x => x.i, x => x.Item1)
                .ToDictionary(x => x.Key, x => x.Max());

            foreach (var row in rows)
            {
                var s =
                    string.Join(' ',
                        row.Select((c, i) => (c ?? string.Empty).PadRight(columnWidthDictionary[i]))
                            .Where(x => x.Length > 0));

                yield return s;
            }
        }


    }
}
