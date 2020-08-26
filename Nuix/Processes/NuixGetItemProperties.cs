using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes.Attributes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// A process that the searches a case for items and outputs the values of item properties.
    /// The report is in CSV format. The headers are 'Key', 'Value', 'Path' and 'Guid'
    /// Use this inside a WriteFile process to write it to a file.
    /// </summary>
    public sealed class NuixGetItemPropertiesProcessFactory : RubyScriptProcessFactory<NuixGetItemProperties, string>
    {
        private NuixGetItemPropertiesProcessFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptProcessFactory<NuixGetItemProperties, string> Instance { get; } = new NuixGetItemPropertiesProcessFactory();

        /// <inheritdoc />
        public override Version RequiredVersion { get; } = new Version(6, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature>();

    }


    /// <summary>
    /// A process that the searches a case for items and outputs the values of item properties.
    /// The report is in CSV format. The headers are 'Key', 'Value', 'Path' and 'Guid'
    /// Use this inside a WriteFile process to write it to a file.
    /// </summary>
    public sealed class NuixGetItemProperties : RubyScriptProcessTyped<string>
    {
        /// <inheritdoc />
        public override IRubyScriptProcessFactory RubyScriptProcessFactory => NuixGetItemPropertiesProcessFactory.Instance;

        ///// <inheritdoc />
        //[EditorBrowsable(EditorBrowsableState.Never)]
        //public override string GetName() => "Get particular properties";


        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [RunnableProcessProperty]
        [Example("C:/Cases/MyCase")]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public IRunnableProcess<string> CasePath { get; set; }

        /// <summary>
        /// The term to search for.
        /// </summary>
        [Required]
        [Example("*.txt")]
        [RunnableProcessProperty]
        public IRunnableProcess<string> SearchTerm { get; set; }


        /// <summary>
        /// The regex to search the property for.
        /// </summary>
        [Example("Date")]
        [Required]
        [RunnableProcessProperty]
        public IRunnableProcess<string> PropertyRegex { get; set; }

        /// <summary>
        /// An optional regex to check the value.
        /// If this is set, only values which match this regex will be returned, and only the contents of the first capture group.
        /// </summary>
        [Example(@"(199\d)")]
        [RunnableProcessProperty]
        public IRunnableProcess<string>? ValueRegex { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.


        /// <inheritdoc />
        internal override string ScriptText => @"
    the_case = utilities.case_factory.open(casePathArg)

    puts ""Finding Entities""
    items = the_case.search(searchArg, {})
    puts ""#{items.length} items found""
    propertyRegex = Regexp.new(propertyRegexArg)
    valueRegex = nil
    valueRegex = Regexp.new(valueRegexArg) if valueRegexArg != nil

    text = ""Key\tValue\tPath\tGuid""

    items.each do |i|
        i.getProperties().each do |k,v|
            begin
                if propertyRegex =~ k
                    if valueRegex != nil
                        if match = valueRegex.match(k) #Only output if the value regex actually matches
                            valueString = match.captures[0]
                            text << ""\n#{k}\t#{valueString}\t#{i.getPathNames().join(""/"")}\t#{i.getGuid()}""
                        end
                    else #output the entire value
                        text << ""\n#{k}\t#{v}\t#{i.getPathNames().join(""/"")}\t#{i.getGuid()}""
                    end
                end
            rescue
            end
        end
    end

    the_case.close
    return text";

        /// <inheritdoc />
        public override string MethodName => "GetParticularProperties";

        /// <inheritdoc />
        internal override IEnumerable<(string argumentName, IRunnableProcess? argumentValue, bool valueCanBeNull)> GetArgumentValues()
        {
            yield return ("casePathArg", CasePath, false);
            yield return ("searchArg", SearchTerm, false);
            yield return ("propertyRegexArg", PropertyRegex, false);
            yield return ("valueRegexArg", ValueRegex, true);
        }

        /// <inheritdoc />
        public override bool TryParse(string s, out string result)
        {
            result = s;
            return true;
        }
    }
}