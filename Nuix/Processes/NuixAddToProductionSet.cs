using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// Searches a case with a particular search string and adds all items it finds to a production set.
    /// Will create a new production set if one with the given name does not already exist.
    /// </summary>
    public sealed class NuixAddToProductionSet : RubyScriptProcess
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => $"Search and add to production set.";


        /// <summary>
        /// The production set to add results to. Will be created if it doesn't already exist
        /// </summary>
        
        [Required]
        [YamlMember(Order = 3)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string ProductionSetName { get; set; }


        /// <summary>
        /// The term to search for
        /// </summary>
        
        [Required]
        [YamlMember(Order = 4)]
        public string SearchTerm { get; set; }

        /// <summary>
        /// The path of the case to search
        /// </summary>
        
        [Required]
        [YamlMember(Order = 5)]
        [ExampleValue("C:/Cases/MyCase")]
        public string CasePath { get; set; }

        /// <summary>
        /// Description of the production set.
        /// </summary>
        [YamlMember(Order = 6)]
        public string? Description { get; set; }

        /// <summary>
        /// How to order the items to be added to the production set.
        /// </summary>
        
        [YamlMember(Order = 7)]
        [ExampleValue("name ASC, item-date DESC")]
        public string? Order { get; set; }

        /// <summary>
        /// The maximum number of items to add to the production set.
        /// </summary>
        [YamlMember(Order = 8)]
        public int? Limit { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        internal override string ScriptName => "AddToProductionSet.rb";
        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-s", SearchTerm);
            yield return ("-n", ProductionSetName);

            if(Description != null)
                yield return ("-d", Description);
            if(Order != null)
                yield return ("-o", Order);
            if(Limit.HasValue)
                yield return ("-l", Limit.Value.ToString());
        }
    }
}