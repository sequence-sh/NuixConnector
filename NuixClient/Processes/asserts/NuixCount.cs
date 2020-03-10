using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using NuixSearch;
using YamlDotNet.Serialization;

namespace NuixClient.processes.asserts
{
    /// <summary>
    /// Asserts that a particular number of items match a particular search term.
    /// </summary>
    public sealed class NuixCount : RubyScriptAssertionProcess
    {
        /// <inheritdoc />
        public override IEnumerable<string> GetArgumentErrors()
        {
            var (searchTermParseSuccess, searchTermParseError, searchTermParsed) = SearchParser.TryParse(SearchTerm);

            if (!searchTermParseSuccess || searchTermParsed == null)
            {
                yield return  $"Error parsing search term: {searchTermParseError}";
            }
        }

        /// <inheritdoc />
        public override string GetName() => $"Assert {ExpectedCount} items match '{SearchTerm}'";

        internal override string ScriptName => "CountItems.rb";

        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-s", SearchTerm);
        }

        private static readonly Regex CountRegex = new Regex( @"\A(?<count>\d+)\sfound\Z", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        protected override (bool success, string? failureMessage)? InterpretLine(string s)
        {
            if (CountRegex.TryMatch(s, out var m) && int.TryParse(m.Groups["count"].Value, out var c))
            {
                return c == ExpectedCount ? (true, null) : (false, $"Expected {ExpectedCount} matches for '{SearchTerm}' but found {c}");
            }

            return null;
        }
        
        /// <inheritdoc />
        protected override string DefaultFailureMessage => "Could not confirm count";


        /// <summary>
        /// If true, asserts that the case does exist.
        /// If false, asserts that the case does not exist.
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 2 )]
        public int ExpectedCount { get; set; }

        /// <summary>
        /// The case path to check
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }

        /// <summary>
        /// The search term to count
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 3)]
        public string SearchTerm { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}