﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    /// <summary>
    /// Json Converters to use for Reductech Entities
    /// </summary>
    public static class JsonConverters
    {
        /// <summary>
        /// All json converters to use for Reductech Entities
        /// </summary>
        public static readonly JsonConverter[] All = {
            StringStreamJsonConverter.Instance,
            EntityJsonConverter.Instance,
            ArrayJsonConverter.Instance,
            new StringEnumConverter()
        };

        /// <summary>
        /// Deserialize a json string into a ConnectionCommand
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public static Result<ConnectionCommand, IErrorBuilder> DeserializeConnectionCommand(string json)
        {
            try
            {
                var command1 = JsonConvert.DeserializeObject<ConnectionCommand>(json)!;

                if (command1.Arguments == null)
                    return command1;

                var newArguments = new Dictionary<string, object>();

                foreach (var (key, value) in command1.Arguments)
                {
                    if (value is JObject jObject)
                    {
                        var entity = JsonConvert.DeserializeObject<Entity>(jObject.ToString()!, EntityJsonConverter.Instance);
                        newArguments.Add(key, entity!);
                    }
                    else if (!(value is string) && value is IEnumerable enumerable)
                    {
                        var newValue = enumerable.OfType<object>().Select(x => x.ToString()).ToList();
                        newArguments.Add(key, newValue);
                    }
                    else
                        newArguments.Add(key, value);
                }

                var command2 = new ConnectionCommand
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

        /// <summary>
        /// Serializes arrays
        /// </summary>
        public class ArrayJsonConverter : JsonConverter
        {
            private ArrayJsonConverter() { }

            /// <summary>
            /// The Instance
            /// </summary>
            public static ArrayJsonConverter Instance { get; } = new ();


            /// <inheritdoc />
            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                if (value is IArray array)
                {
                    var objectsResult = array.GetObjects();
                    if (objectsResult.IsFailure)
                        throw new ErrorException(objectsResult.Error);


                    serializer.Serialize(writer, objectsResult.Value);
                }
            }

            /// <inheritdoc />
            public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public override bool CanConvert(Type objectType) => objectType.IsAssignableTo(typeof(IArray));
        }


        /// <summary>
        /// Convert StringStreams to Json
        /// </summary>
        public class StringStreamJsonConverter : JsonConverter
        {
            private StringStreamJsonConverter() { }

            /// <summary>
            /// The instance
            /// </summary>
            public static StringStreamJsonConverter Instance { get; } = new();

            /// <inheritdoc />
            public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
            {
                if (value is StringStream ss)
                {
                    var s = ss.GetString();
                    serializer.Serialize(writer, s);
                }
            }

            /// <inheritdoc />
            public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc />
            public override bool CanConvert(Type objectType) => objectType == typeof(StringStream);
        }


        /// <summary>
        /// Converts Entities to Json
        /// </summary>
        private class EntityJsonConverter : JsonConverter
        {
            private EntityJsonConverter() { }

            /// <summary>
            /// The instance.
            /// </summary>
            public static EntityJsonConverter Instance { get; } = new();


            /// <inheritdoc />
            public override void WriteJson(JsonWriter writer, object? entityObject, JsonSerializer serializer)
            {
                if (entityObject is not Entity entity) return;
                var dictionary = new Dictionary<string, object?>();

                foreach (var entityProperty in entity)
                {
                    var value = GetObject(entityProperty.BestValue);
                    dictionary.Add(entityProperty.Name, value);
                }

                serializer.Serialize(writer, dictionary);

                static object? GetObject(EntityValue ev)
                {
                    return ev.Value.Match(
                        _ => null as object,
                        x => x,
                        x => x,
                        x => x,
                        x => x,
                        x => x.Value,
                        x => x,
                        x => x,
                        x => x.Select(GetObject).ToList()
                    );
                }
            }

            /// <inheritdoc />
            public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
            {
                var objectDict = serializer.Deserialize<Dictionary<string, object>>(reader);
                var entity = Entity.Create(objectDict!.Select(x => (x.Key, x.Value))!);

                return entity;
            }

            /// <inheritdoc />
            public override bool CanConvert(Type objectType) => objectType == typeof(Entity);
        }
    }
}