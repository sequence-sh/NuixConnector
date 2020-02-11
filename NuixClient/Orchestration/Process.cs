using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NuixClient.Orchestration
{
    public class ProcessJsonConverter : JsonConverter
    {

        private static readonly IReadOnlyDictionary<string, Type> ProcessTypeDictionary = new[]
        {
            typeof(AddConcordanceProcess),
            typeof(AddFileProcess),
            typeof(BranchProcess),
            typeof(CreateCaseProcess),
            typeof(ExportConcordanceProcess),
            typeof(MultiStepProcess),
            typeof(SearchAndTagProcess)
        }.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var typeProperty = jo.Property(nameof(Process.Type));

            if (typeProperty == null)
                throw new JsonReaderException("Could not find type property");

            var typeString = typeProperty.Value.ToString();

            if(!ProcessTypeDictionary.TryGetValue(typeString, out var realObjectType))
                throw new JsonReaderException($"Could not process with type '{typeString}'");

            var instance = Activator.CreateInstance(realObjectType);

            foreach (var jp in jo.Properties().Where(x => x.Name != nameof(Condition.Type)))
            {
                var property = realObjectType.GetProperty(jp.Name);
                if( property == null)
                    throw new JsonReaderException($"{typeString} has no '{jp.Name}' property");

                property.SetValue(instance, jp.Value.ToObject(property.PropertyType, serializer));
            }

            return instance;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(Process));
        }
    }

    /// <summary>
    /// A process. Can contain one or more steps
    /// </summary>
    public abstract class Process
    {
        /// <summary>
        /// The type of this process
        /// </summary>
        [JsonProperty(Order = 1)]
        public string Type => GetType().Name;

        /// <summary>
        /// Conditions which must be true for this process to be executed
        /// </summary>
        [JsonProperty(Order = 2)]
        public List<Condition>? Conditions { get; set; }

        /// <summary>
        /// The name of this process
        /// </summary>
        public abstract string GetName();

        /// <summary>
        /// Executes this process. Should only be called if all conditions are met
        /// </summary>
        /// <returns></returns>
        public abstract IAsyncEnumerable<ResultLine> Execute();

        /// <summary>
        /// String representation of this process
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetName();
        }
    }
}