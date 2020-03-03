using System.Collections.Generic;
using System.Linq;

namespace InstantConsole
{
    internal static class Prettifier
    {
        internal static IEnumerable<string> ArrangeIntoColumns(IEnumerable<IReadOnlyCollection<string?>> rows)
        {
            var data = rows.SelectMany((row, rowNumber) =>
                row.SelectMany((cell, columnNumber) =>
                    (cell ?? string.Empty).Split("\n")
                    .Select((line, lineNumber) => (rowNumber, lineNumber, columnNumber, line))
                )).ToList();

            var columnWidthDictionary = data.GroupBy(x => x.columnNumber)
                .ToDictionary(x => x.Key, x => x.Max(y => y.line.Length));


            foreach  (var grouping in 
                data.GroupBy(d=>(d.rowNumber, d.lineNumber))
                    
                    .OrderBy(x=>x.Key.rowNumber)
                    .ThenBy(x=>x.Key.lineNumber)
                )
            {

                var terms = new List<string>();
                var i = 0;
                foreach (var (_, _, columnNumber, line) in grouping.OrderBy(r => r.columnNumber))
                {
                    while (i < columnNumber)
                    {
                        terms.Add( new string(' ', columnWidthDictionary[i]));
                        i++;
                    }

                    terms.Add( line.PadRight(columnWidthDictionary[columnNumber]));
                    i++;
                }

                var s = string.Join(' ', terms);
                yield return s;
            }
        }
    }
}
