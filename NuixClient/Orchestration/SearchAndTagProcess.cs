using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using YamlDotNet.Serialization;

namespace NuixClient.Orchestration
{
    /// <summary>
    /// A process which searches a case with a particular search string and tags all files it finds
    /// </summary>
    internal class SearchAndTagProcess : RubyScriptProcess1
    {
        /// <summary>
        /// The name of this process
        /// </summary>
        public override string GetName() => $"Search and Tag with '{Tag}'";


        /// <summary>
        /// The tag to assign to found results
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string Tag { get; set; }


        /// <summary>
        /// The term to search for
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 4)]
        public string SearchTerm { get; set; }

        /// <summary>
        /// The path of the case to search
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 5)]
        public string CasePath { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        internal override IEnumerable<string> GetArgumentErrors()
        {
            var (searchTermParseSuccess, searchTermParseError, searchTermParsed) = Search.SearchParser.TryParse(SearchTerm);

            if (!searchTermParseSuccess || searchTermParsed == null)
            {
                yield return  $"Error parsing search term: {searchTermParseError}";
            }
        }

        internal override string ScriptName => "SearchAndTag.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-s", SearchTerm);
            yield return ("t", Tag);

            //TODO limit and order
        }
    }
}