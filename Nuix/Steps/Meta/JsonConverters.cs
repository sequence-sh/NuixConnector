using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{

/// <summary>
/// Json Converters to use for Reductech Entities
/// </summary>
public static class JsonConverters
{
    /// <summary>
    /// Json options to use
    /// </summary>
    public static readonly JsonSerializerOptions Options = new()
    {
        Converters =
        {
            StringStreamJsonConverter.Instance,
            EntityJsonConverter.Instance,
            StringEnumDisplayConverter.Instance,
            ArrayJsonConverter.Instance
        }
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
            var command1 = JsonSerializer.Deserialize<ConnectionCommand>(json, Options)!;

            if (command1.Arguments == null)
                return command1;

            var newArguments = new Dictionary<string, object>();

            foreach (var (key, value) in command1.Arguments)
            {
                if (value is JsonElement jElement)
                {
                    var entity = Entity.Create(jElement);

                    newArguments.Add(key, entity);
                }
                else if (value is not string && value is IEnumerable enumerable)
                {
                    var newValue = enumerable.OfType<object>().Select(x => x.ToString()).ToList();
                    newArguments.Add(key, newValue);
                }
                else
                    newArguments.Add(key, value);
            }

            var command2 = new ConnectionCommand
            {
                Arguments          = newArguments,
                Command            = command1.Command,
                FunctionDefinition = command1.FunctionDefinition,
                IsStream           = command1.IsStream
            };

            return command2;
        }
        catch (JsonException e)
        {
            return new ErrorBuilder(e, ErrorCode.ExternalProcessError);
        }
    }

    /// <summary>
    /// Convert StringStreams to Json
    /// </summary>
    public class StringStreamJsonConverter : JsonConverter<StringStream>
    {
        private StringStreamJsonConverter() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static StringStreamJsonConverter Instance { get; } = new();

        /// <inheritdoc />
        public override bool CanConvert(Type objectType) => objectType == typeof(StringStream);

        /// <inheritdoc />
        public override StringStream Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            return reader.GetString() ?? new StringStream("");
        }

        /// <inheritdoc />
        public override void Write(
            Utf8JsonWriter writer,
            StringStream value,
            JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.GetString());
        }
    }

    /// <summary>
    /// Converts enums using the enum display property
    /// </summary>
    public class StringEnumDisplayConverter : JsonConverter<Enum>
    {
        private StringEnumDisplayConverter() { }

        /// <summary>
        /// The instance
        /// </summary>
        public static JsonConverter<Enum> Instance { get; } = new StringEnumDisplayConverter();

        /// <inheritdoc />
        public override bool CanConvert(Type typeToConvert)
        {
            Type t = Nullable.GetUnderlyingType(typeToConvert) ?? typeToConvert;
            return t.IsEnum;
        }

        /// <inheritdoc />
        public override Enum? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var  nullableObjectType = Nullable.GetUnderlyingType(typeToConvert);
            Type t                  = nullableObjectType ?? typeToConvert;

            var enumText = reader.GetString();

            try

            {
                if (string.IsNullOrWhiteSpace(enumText) && nullableObjectType != null)
                    return null;

                return (Enum)Enum.Parse(t, enumText!, true);
            }
            catch
            {
                throw new JsonException(
                    $"Error converting value {enumText} to type '{{{t.Name}}}'."
                );
            }
        }

        /// <inheritdoc />
        public override void Write(Utf8JsonWriter writer, Enum value, JsonSerializerOptions options)
        {
            var s = value.GetDisplayName();
            writer.WriteStringValue(s);
        }
    }

    /// <summary>
    /// Serializes arrays
    /// </summary>
    public class ArrayJsonConverter : JsonConverter<IArray>
    {
        /// <summary>
        /// The Instance
        /// </summary>
        public static ArrayJsonConverter Instance { get; } = new();

        /// <inheritdoc />
        public override bool CanConvert(Type objectType) =>
            objectType.IsAssignableTo(typeof(IArray));

        /// <inheritdoc />
        public override IArray? Read(
            ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public override void Write(
            Utf8JsonWriter writer,
            IArray value,
            JsonSerializerOptions options)
        {
            var objectsResult = value.GetObjectsAsync(CancellationToken.None).Result;

            if (objectsResult.IsFailure)
                throw new ErrorException(objectsResult.Error);

            JsonSerializer.Serialize(writer, objectsResult.Value, options);
        }
    }
}

}
