using System;

namespace Razor_Components.Inputs
{
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
}