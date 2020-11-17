using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Entity = Reductech.EDR.Core.Entities.Entity;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    /// <summary>
    /// Converts Entities to Json
    /// </summary>
    public class EntityJsonConverter : JsonConverter
    {
        /// <summary>
        /// Deserialize a json string into a ConnectionCommand
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Result<ConnectionCommand, IErrorBuilder> DeserializeConnectionCommand(string json)
        {
            try
            {
                var command1 = JsonConvert .DeserializeObject<ConnectionCommand>(json)!;

                if (command1.Arguments == null || !command1.Arguments.Any(x => x.Value is JObject))
                    return command1;

                var newArguments = new Dictionary<string, object>();

                foreach (var (key, value) in command1.Arguments)
                {
                    if (value is JObject)
                    {
                        var entity = JsonConvert.DeserializeObject<Entity>(value.ToString()!, Instance);
                        newArguments.Add(key, entity!);
                    }
                    else
                        newArguments.Add(key, value);
                }


                var command2 = new ConnectionCommand()
                {
                    Arguments = newArguments,
                    Command = command1.Command,
                    FunctionDefinition = command1.FunctionDefinition,
                    IsStream = command1.IsStream
                };

                return command2;
            }
            catch (JsonException e)
            {
                return new ErrorBuilder(e, ErrorCode.ExternalProcessError);
            }
        }



        private EntityJsonConverter() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static JsonConverter Instance { get; } = new EntityJsonConverter();

        /// <inheritdoc />
        public override void WriteJson(JsonWriter writer, object? entityObject, JsonSerializer serializer)
        {
            if (!(entityObject is Entity entity))
                return;

            var dictionary = new Dictionary<string, object?>();

            foreach (var (key, value) in entity)
            {
                value.Value.Switch(
                    _ => { dictionary.Add(key, null); },
                    x => dictionary.Add(key, GetObject(x)),
                    x => dictionary.Add(key, GetList(x)));
            }

            serializer.Serialize(writer, dictionary);

            static List<object?> GetList(IEnumerable<EntitySingleValue> source)
            {
                var r = source.Select(GetObject).ToList();
                return r;
            }

            static object? GetObject(EntitySingleValue esv)
            {
                object? o = null;

                esv.Value.Switch(
                    a => o = a,
                    a => o = a,
                    a => o = a,
                    a => o = a,
                    a => o = a,
                    a => o = a
                );

                return o;
            }


        }

        /// <inheritdoc />
        public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            var evDict = new Dictionary<string, EntityValue>();

            var objectDict = serializer.Deserialize<Dictionary<string, object>>(reader);

            foreach (var (key, value) in objectDict!)
            {
                EntityValue ev;

                if (!(value is string) && value is IEnumerable enumerable)
                {
                    var list = enumerable.OfType<object>().Select(x => x.ToString()!).ToList();
                    ev = EntityValue.Create(list);
                }
                else
                    ev = EntityValue.Create(value.ToString());

                evDict.Add(key, ev);
            }

            var entity = new Entity(evDict);

            return entity;
        }

        /// <inheritdoc />
        public override bool CanConvert(Type objectType) => objectType == typeof(Entity);
    }
}