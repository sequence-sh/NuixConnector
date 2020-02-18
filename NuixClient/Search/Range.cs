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
        private Range(int? min, string? minUnits, int? max, string? maxUnits)
        {
            Min = min;
            Max = max;
            MinUnits = minUnits;
            MaxUnits = maxUnits;
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

                var minUnits = match.Groups["sUnits"].Value;
                var maxUnits = match.Groups["eUnits"].Value;

                if (!start.HasValue && !end.HasValue)
                    range = null;
                else range = new Range(start, minUnits, end, maxUnits);
            }
            else
                range = null;

            return range != null;
        }

        internal static readonly Regex RangeRegex = new Regex(
            @"\A\[\s*(?:(?<start>-?\d+)(?<sUnits>Y)?|\*)\s*[tT][oO]\s*(?:(?<end>-?\d+)(?<eUnits>Y)?|\*)\s*\]\Z", RegexOptions.Compiled);

        /// <summary>
        /// The minimum value
        /// </summary>
        public readonly int? Min;

        /// <summary>
        /// Units of the minimum value
        /// </summary>
        public readonly string? MinUnits;

        /// <summary>
        /// The maximum value
        /// </summary>
        public readonly int? Max;

        /// <summary>
        /// Units of the maximum value
        /// </summary>
        public readonly string? MaxUnits;

        /// <summary>
        /// Converts this range to a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[{AsString(Min, MinUnits)} TO {AsString(Max, MaxUnits)}]";

            static string AsString(int? i, string? units)
            {
                return i.HasValue ? i.Value.ToString() + units : "*";
            }
        }

        
    }
}