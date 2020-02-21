using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Orchestration.Conditions;
using Orchestration.Enumerations;
using Orchestration.Processes;
using YamlDotNet.Serialization;

namespace Orchestration
{
    public static class YamlHelper
    {
        private static IEnumerable<Type> SpecialTypes
        {
            get
            {
                var types = AppDomain.CurrentDomain.GetAssemblies()
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


        public static string ConvertToYaml(Process process)
        {
            var r = Serializer.Value.Serialize(process);

            return r;
        }

        [ContractAnnotation("=>true,process:notNull,error:Null; =>false,process:null,error:notNull")]
        public static bool TryMakeFromYaml(string yaml, out Process? process, out string? error)
        {
            try
            {
                process = Deserializer.Value.Deserialize<Process>(yaml);
                error = null;
                return true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                process = null;
                error = e.Message;
                return false;
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }
}