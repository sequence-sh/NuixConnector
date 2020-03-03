using System;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Orchestration.Conditions;
using Orchestration.Enumerations;
using Orchestration.Processes;
using YamlDotNet.Serialization;

namespace Orchestration
{
    /// <summary>
    /// Contains methods for serializing and deserializing yaml
    /// </summary>
    public static class YamlHelper
    {
        private static IEnumerable<Type> SpecialTypes
        {
            get
            {
                var assemblies = AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(OrchestrationModuleAttribute)))
                    .ToList();

                var types = assemblies
                    .SelectMany(s => s.GetTypes())
                    .Where(type => 
                        typeof(Process).IsAssignableFrom(type) 
                        || typeof(Condition).IsAssignableFrom(type) 
                        || typeof(Enumeration).IsAssignableFrom(type))
                    .Where(x=>!x.IsAbstract && ! x.IsInterface)
                    .ToList();

                return types;
            }
        }

        private static readonly Lazy<IDeserializer> Deserializer = new Lazy<IDeserializer>(() =>
        {
            var deSerializerBuilder = new DeserializerBuilder();
            deSerializerBuilder =
                SpecialTypes.Aggregate(deSerializerBuilder, (current, specialType) => current.WithTagMapping("!" + specialType.Name, specialType));

            return  deSerializerBuilder.Build();
        });

        private static readonly Lazy<ISerializer> Serializer = new Lazy<ISerializer>(() =>
        {
            var serializerBuilder = new SerializerBuilder();
            serializerBuilder =
                SpecialTypes.Aggregate(serializerBuilder, (current, specialType) => current.WithTagMapping("!" + specialType.Name, specialType));
            serializerBuilder.ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull);
            return serializerBuilder.Build();
        });

        /// <summary>
        /// Creates a Yaml string representing a process.
        /// </summary>
        public static string ConvertToYaml(Process process)
        {
            var r = Serializer.Value.Serialize(process);

            return r;
        }

        /// <summary>
        /// Tries to create a process from a Yaml string.
        /// </summary>
        public static Result<Process> TryMakeFromYaml(string yaml)
        {
            return  Result.Try(() => Deserializer.Value.Deserialize<Process>(yaml), e => e.Message);
        }
    }
}