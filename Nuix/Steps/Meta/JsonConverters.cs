using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using Reductech.Sequence.Core.Entities;
using Reductech.Sequence.Core.Internal.Errors;

namespace Reductech.Sequence.Connectors.Nuix.Steps.Meta
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
            var s = value.GetString();

            writer.WriteStringValue(s);
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
        public override IArray Read(
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

    /// <summary>
    /// Convert a json element to an object
    /// </summary>
    public static object ConvertToObject(JsonElement jElement)
    {
        return jElement.ValueKind switch
        {
            JsonValueKind.Array  => jElement.EnumerateArray().Select(ConvertToObject).ToList(),
            JsonValueKind.String => jElement.GetString()!,
            JsonValueKind.Number => jElement.GetDouble(),
            _                    => Core.Entity.Create(jElement)
        };
    }
}

}
