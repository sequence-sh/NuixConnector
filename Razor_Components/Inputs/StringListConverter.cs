using System;
using System.Collections.Generic;
using System.Linq;

namespace Razor_Components.Inputs
{
    internal class StringListConverter : IConverter
    {
        Type IConverter.ParameterType => typeof(List<string>);

        string IConverter.FromObject(object? o)
        {
            if (o != null && o is IEnumerable<string> enumerable)
            {
                return string.Join("\n", enumerable);
            }
            else return string.Empty;
        }

        (bool success, object? result) IConverter.FromString(string s)
        {
            return (true, s.Split("\n").ToList());
        }

        string IConverter.InputType => "text";

        string? IConverter.InputPattern => null;
    }
}