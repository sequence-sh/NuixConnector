using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace NuixClient.Orchestration
{
    public class ConditionJsonConverter : JsonConverter
    {
        private static readonly IReadOnlyDictionary<string, Type> ConditionTypeDictionary = new[]
        {
            typeof(FileExistsCondition)
        }.ToDictionary(x => x.Name, StringComparer.OrdinalIgnoreCase);

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var jo = JObject.Load(reader);
            var typeProperty = jo.Property(nameof(Condition.Type));

            if (typeProperty == null)
                throw new JsonReaderException("Could not find type property");

            var typeString = typeProperty.Value.ToString();

            if (!ConditionTypeDictionary.TryGetValue(typeString, out var realObjectType))
                throw new JsonReaderException($"Could not process with type '{typeString}'");

            var instance = Activator.CreateInstance(realObjectType);

            foreach (var jp in jo.Properties().Where(x => x.Name != nameof(Condition.Type)))
            {
                var property = realObjectType.GetProperty(jp.Name);
                if (property == null)
                    throw new JsonReaderException($"{typeString} has no '{jp.Name}' property");

                property.SetValue(instance, jp.Value.ToObject(property.PropertyType, serializer));
            }

            return instance;
        }

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsAssignableFrom(typeof(Condition));
        }
    }

    /// <summary>
    /// A condition that is required for the process to execute
    /// </summary>
    public abstract class Condition
    {
        /// <summary>
        /// The type of this condition
        /// </summary>
        [JsonProperty(Order = 1)]
        public string Type => GetType().Name;


        /// <summary>
        /// Description of this condition
        /// </summary>
        public abstract string GetDescription();


        /// <summary>
        /// String representation of this Description
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return GetDescription();
        }

        /// <summary>
        /// Is this condition met
        /// </summary>
        public abstract bool IsMet();

        
    }
}