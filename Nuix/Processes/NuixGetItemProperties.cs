using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// A process that the searches a case for items and outputs the values of item properties.
    /// </summary>
    public sealed class NuixGetItemProperties : RubyScriptWithOutputProcess
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => "Get particular properties";

        internal override string ScriptName => "GetParticularProperties.rb";


        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [YamlMember(Order = 2)]
        [ExampleValue("C:/Cases/MyCase")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }

        /// <summary>
        /// The term to search for.
        /// </summary>
        [Required]
        [ExampleValue("*.txt")]
        [YamlMember(Order = 3)]
        public string SearchTerm { get; set; }


        /// <summary>
        /// The term to search for.
        /// </summary>
        [ExampleValue("Date")]
        [Required]
        [YamlMember(Order = 5)]
        public string PropertyRegex { get; set; }

        /// <summary>
        /// The name of the text file to write the results to.
        /// Should not include the extension.
        /// </summary>
        [Required]
        [ExampleValue("Results")]
        [YamlMember(Order = 6)]
        public string OutputFileName { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        internal override IEnumerable<(string arg, string val)> GetArgumentValuePairs()
        {
            yield return ("-p", CasePath);
            yield return ("-s", SearchTerm);
            yield return ("-r", PropertyRegex);
            yield return ("-f", OutputFileName);
        }
    }
}