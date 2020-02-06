using System;

namespace Razor_Components.Inputs
{
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
}