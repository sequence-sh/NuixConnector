using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{


    /// <summary>
    /// Creates a list of all terms appearing in the case and their frequencies.
    /// </summary>
    public sealed class NuixCreateTermList : RubyScriptProcess
    {
        /// <inheritdoc />
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string GetName() => "Create Termlist";

        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [YamlMember(Order = 5)]
        [ExampleValue("C:/Cases/MyCase")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string CasePath { get; set; }


        /// <summary>
        /// The path to the folder to put the output files in.
        /// </summary>
        [Required]
        [ExampleValue("C:/Output")]
        [YamlMember(Order = 4)]
        public string OutputFolder { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        


        /// <inheritdoc />
        internal override string ScriptText => @"   the_case = utilities.case_factory.open(casePathArg)

    puts ""Generating Report:""   

    caseStatistics = the_case.getStatistics()

    termStatistics = caseStatistics.getTermStatistics("""", {""sort"" => ""on"", ""deduplicate"" => ""md5""}) #for some reason this takes strings rather than symbols
    #todo terms per custodian
    puts ""#{termStatistics.length} terms""

    text = ""Terms:Term\tCount""

    termStatistics.each do |term, count|
        text << ""\n#{term}\t#{count}""
    end

    File.write(outputFilePathArg, text)
   
    the_case.close";

        /// <inheritdoc />
        internal override string MethodName => "CreateTermList";

        /// <summary>
        /// The name of the file that will be created.
        /// </summary>
        public const string FileName = "terms.txt";

        /// <inheritdoc />
        internal override IEnumerable<(string arg, string? val, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("casePathArg", CasePath, false);

            var fullFilePath = Path.Combine(OutputFolder, FileName);

            yield return ("outputFilePathArg", fullFilePath, false);
        }
    }
}