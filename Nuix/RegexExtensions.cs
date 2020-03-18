using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reductech.EDR.Connectors.Nuix
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
}
