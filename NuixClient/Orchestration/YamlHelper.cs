using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using YamlDotNet.Serialization;

namespace NuixClient.Orchestration
{
    internal static class YamlHelper
    {
        private static readonly IReadOnlyList<Type> SpecialTypes = new[]
        {
            typeof(AddConcordanceProcess),
            typeof(AddFileProcess),
            typeof(BranchProcess),
            typeof(CreateCaseProcess),
            typeof(ExportConcordanceProcess),
            typeof(MultiStepProcess),
            typeof(SearchAndTagProcess),
            typeof(AddToProductionSetProcess),
            typeof(CreateReportProcess),
            typeof(CreateTermlistProcess),

            typeof(FileExistsCondition)
        };


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
            catch (Exception e)
            {
                process = null;
                error = e.Message;
                return false;
            }
        }
    }
}