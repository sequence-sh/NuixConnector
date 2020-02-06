using System;

namespace Razor_Components.Inputs
{
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
}