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


        /// <inheritdoc />
        internal override string ScriptText => @"   the_case = utilities.case_factory.open(pathArg)

    puts ""Finding Entities""
    items = the_case.search(searchArg, {})
    puts ""#{items.length} items found""
    regex = Regexp.new(regexArg)    
    puts ""Output#{fileArg}:Key\tValue\tPath\tGuid""

    items.each do |i| 
        i.getProperties().each do |k,v|
          puts ""Output#{fileArg}:#{k}\t#{v}\t#{i.getPathNames().join(""/"")}\t#{i.getGuid()}"" if regex =~ k
        end
    end
   
    the_case.close";

        /// <inheritdoc />
        internal override string MethodName => "GetParticularProperties";

        /// <inheritdoc />
        internal override IEnumerable<(string arg, string? val, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("pathArg", CasePath, false);
            yield return ("searchArg", SearchTerm, false);
            yield return ("regexArg", PropertyRegex, false);
            yield return ("fileArg", OutputFileName, false);
        }
    }
}