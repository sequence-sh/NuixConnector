using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using Orchestration.Conditions;
using Orchestration.Enumerations;
using YamlDotNet.Serialization;

namespace Orchestration.Processes
{
    /// <summary>
    /// Performs a nested process once for each element in an enumeration
    /// </summary>
    public class ForEach : Process
    {
        public override IEnumerable<string> GetArgumentErrors()
        {
            return SubProcess.GetArgumentErrors(); //TODO look at this - its problematic. There seems to be no way to check the injected argument
        }

        public override string GetName() => $"Foreach in {Enumeration}, {SubProcess}";

        [Required]
        [DataMember]
        [YamlMember(Order = 2)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public Enumeration Enumeration { get; set; }

        /// <summary>
        /// The property of the subProcess to inject with the element of enumeration
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 3)]
        public string PropertyToInject { get; set; }


        /// <summary>
        /// The template to apply to the element before injection.
        /// If null the element will be used without modification
        /// The string '$s' in the template will be replaced with the element
        /// </summary>
        [DataMember]
        [YamlMember(Order = 4)]
        public string? Template { get; set; }

        /// <summary>
        /// The process to run once for each element
        /// </summary>
        [Required]
        [DataMember]
        [YamlMember(Order = 5)]
        public Process SubProcess { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        public override async IAsyncEnumerable<ResultLine> Execute()
        {
            foreach (var element in Enumeration.Elements.ToList())
            {
                var elementS = Template == null ? element : Template.Replace("$s", element);

                var subProcess = SubProcess; //TODO if we ever try to run these in parallel we will need to clone the process

                var property = subProcess.GetType().GetProperty(PropertyToInject); //TODO handle dots and array indexes in this argument

                if (property == null)
                {
                    yield return new ResultLine(false, $"Could not find property '{PropertyToInject}'");
                    break;
                }

                property.SetValue(subProcess, elementS);

                if ((subProcess.Conditions??Enumerable.Empty<Condition>()).All(x => x.IsMet()))
                {
                    var resultLines = subProcess.Execute();

                    await foreach (var rl in resultLines)
                        yield return rl;
                }
            }
        }
    }
}
