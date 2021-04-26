using System.Text.RegularExpressions;

namespace Reductech.EDR.Connectors.Nuix
{

internal static class RegexExtensions
{
    public static bool TryMatch(this Regex r, string input, out Match match)
    {
        match = r.Match(input);
        return match.Success;
    }
}

}
