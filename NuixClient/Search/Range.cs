using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace NuixClient.Search
{
    /// <summary>
    /// A range of integers
    /// </summary>
    public class Range
    {
        /// <summary>
        /// Create a new range
        /// </summary>
        public Range(int? min, int? max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Create a range from a string
        /// </summary>
        /// <returns></returns>
        [ContractAnnotation("=>true,range:notNull; =>false,range:null;")]
        public static bool TryParse(string str, out Range? range)
        {
            
            var match = RangeRegex.Match(str);

            if (match.Success)
            {
                var start = int.TryParse(match.Groups["start"].Value, out var s) ? s : null as int?;
                var end   = int.TryParse(match.Groups["end"].Value, out var e) ? e : null as int?;

                if (!start.HasValue && !end.HasValue)
                    range = null;
                else range = new Range(start, end);
            }
            else
                range = null;

            return range != null;
        }

        private static readonly Regex RangeRegex = new Regex(
            @"\A\[\s*(?<start>\d+|\*)\s*TO\s*(?<end>\d+|\*)\s*\]\Z", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// The minimum value
        /// </summary>
        public readonly int? Min;
        /// <summary>
        /// The maximum value
        /// </summary>
        public readonly int? Max;

        /// <summary>
        /// Converts this range to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[{AsString(Min)} TO {AsString(Max)}]";

            static string AsString(int? i)
            {
                return i.HasValue ? i.Value.ToString() : "*";
            }
        }

        
    }
}