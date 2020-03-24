﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes.asserts
{
    /// <summary>
    /// A process that succeed if the numbers of items returned by a search is within a particular range and fails if it is not.
    /// Useful in Conditionals.
    /// </summary>
    public sealed class NuixCount : RubyScriptAssertionProcess
    {
        /// <summary>
        /// Inclusive minimum of the expected range.
        /// Either this, Maximum, or both must be set.
        /// </summary>
        
        [YamlMember(Order = 2 )]
        public int? Minimum { get; set; }

        /// <summary>
        /// Inclusive maximum of the expected range.
        /// Either this, Minimum, or both must be set.
        /// </summary>
        
        [YamlMember(Order = 3 )]
        public int? Maximum { get; set; }

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [YamlMember(Order = 4)]
        [ExampleValue("C:/Cases/MyCase")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }

        /// <summary>
        /// The search term to count.
        /// </summary>
        [Required]
        [ExampleValue("*.txt")]
        [YamlMember(Order = 5)]
        public string SearchTerm { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        internal override string ScriptText => @"   the_case = utilities.case_factory.open(pathArg)

    puts ""Counting '#{searchArg}'""
    searchOptions = {}
    count = the_case.count(searchArg, searchOptions)
    puts ""#{count} found""
    the_case.close";

        /// <inheritdoc />
        internal override string MethodName => "CountItems";

        /// <inheritdoc />
        internal override IEnumerable<(string arg, string? val, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("searchArg", SearchTerm, false);
        }

        /// <inheritdoc />
        internal override IEnumerable<string> GetAdditionalArgumentErrors()
        {
            if(Minimum == null && Maximum == null)
                yield return  "Either minimum or maximum must be set.";
        }

        /// <inheritdoc />
        public override string GetName()
        {
            if (Minimum == null)
                return Maximum == null
                    ? $"Assert count of '{SearchTerm}'"
                    : $"Assert count of '{SearchTerm}' <= {Maximum.Value}";
            return Maximum == null
                ? $"Assert {Minimum.Value} <= count of '{SearchTerm}'"
                : $"Assert {Minimum.Value} <= count of '{SearchTerm}' <= {Maximum.Value}";
        } 


        private static readonly Regex CountRegex = new Regex( @"\A(?<count>\d+)\sfound\Z", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <inheritdoc />
        protected override (bool success, string? failureMessage)? InterpretLine(string s)
        {
            if (!CountRegex.TryMatch(s, out var m) || !int.TryParse(m.Groups["count"].Value, out var c)) return null;

            if (Maximum.HasValue && c > Maximum)
                return (false, $"Expected {Maximum.Value} or fewer matches for '{SearchTerm}' but found {c}");

            if (Minimum.HasValue && c < Minimum)
                return (false, $"Expected {Minimum.Value} or more matches for '{SearchTerm}' but found {c}");

            return (true, null);

        }
        
        /// <inheritdoc />
        protected override string DefaultFailureMessage => "Could not confirm count";
    }
}