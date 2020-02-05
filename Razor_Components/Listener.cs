using System;
using System.Collections.Concurrent;

namespace Razor_Components
{
    internal interface IConverter
    {
        Type ParameterType { get; }

        string FromObject(object? o);
        (bool success, object? result) FromString(string s);

        string InputType { get; }

        string? InputPattern { get; }
    }

    internal class EnumConverter : IConverter
    {
        public EnumConverter(Type parameterType)
        {
            ParameterType = parameterType;
        }

        public Type ParameterType { get; }

        public string FromObject(object? o)
        {
            return o?.ToString() ?? string.Empty;
        }

        public (bool success, object? result) FromString(string s)
        {
            var result = (Enum.TryParse(ParameterType, s, true, out var r), r);
            return result;
        }

        public string InputType => "text";
        public string? InputPattern => null;
    }

    internal class StringConverter : IConverter
    {
        Type IConverter.ParameterType => typeof(string);

        string IConverter.FromObject(object? o)
        {
           return  o?.ToString() ?? string.Empty;
        }

        (bool success, object? result) IConverter.FromString(string s)
        {
            return (true, s);
        }

        public string InputType => "text";
        public string? InputPattern => null;
    }

    internal class GenericConverter<T> : IConverter where T : struct
    {

        private readonly Func<string, (bool success, T? result)> _parseFunc;

        public GenericConverter(Func<string, (bool success, T? result )> parseFunc, string inputType, string? inputPattern)
        {
            _parseFunc = parseFunc;
            InputType = inputType;
            InputPattern = inputPattern;
        }

        public Type ParameterType => typeof(T);

        public string FromObject(object? o)
        {
            return o?.ToString() ?? string.Empty;
        }

        public (bool success, object? result ) FromString(string s)
        {

            var r = _parseFunc(s);
            return r;
        }

        public string InputType { get; }
        public string? InputPattern { get; }
    }

    internal class BoolListener
    {
        // ReSharper disable once UnusedMember.Global - this property is bound to by the UI
        public bool? Property
        {
            get
            {
                var o = Dictionary[PropertyName];

                return o switch
                {
                    true => true,
                    false => false,
                    _ => _isNullable? null as bool? : false
                };
            }
            set
            {
                Dictionary.AddOrUpdate(PropertyName, value, (_, __) => value);
            }
        }

        public readonly string PropertyName;

        public readonly ConcurrentDictionary<string, object?> Dictionary;
        private readonly bool _isNullable;

        public BoolListener(string propertyName, ConcurrentDictionary<string, object?> dictionary, bool isNullable)
        {
            PropertyName = propertyName;
            Dictionary = dictionary;
            _isNullable = isNullable;
        }
    }

    internal class Listener
    {
        // ReSharper disable once UnusedMember.Global - this property is bound to by the UI
        public string Property
        {
            get
            {
                var o = Dictionary[PropertyName];

                return _converter.FromObject(o);
            }
            set
            {
                var (success, asObject) = _converter.FromString(value);

                if (success)
                {
                    Dictionary.AddOrUpdate(PropertyName, asObject, (_, __) => asObject);
                }
            }
        }

        public readonly string PropertyName;

        public readonly ConcurrentDictionary<string, object?> Dictionary;
        private readonly IConverter _converter;

        public Listener(string propertyName, ConcurrentDictionary<string, object?> dictionary, IConverter converter)
        {
            PropertyName = propertyName;
            Dictionary = dictionary;
            _converter = converter;
        }
    }
}