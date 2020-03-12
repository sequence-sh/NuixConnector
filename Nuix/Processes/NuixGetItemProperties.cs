using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
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
        /// The path of the case to examine.
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 2)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }

        /// <summary>
        /// The term to search for
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 3)]
        public string SearchTerm { get; set; }


        /// <summary>
        /// The term to search for
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember(Order = 5)]
        public string PropertyRegex { get; set; }

        /// <summary>
        /// The name of the file to write the results to
        /// </summary>
        [DataMember]
        [Required]
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