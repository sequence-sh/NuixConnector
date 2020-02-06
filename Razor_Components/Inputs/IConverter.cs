using System;

namespace Razor_Components.Inputs
{
    internal interface IConverter
    {
        Type ParameterType { get; }

        string FromObject(object? o);
        (bool success, object? result) FromString(string s);

        string InputType { get; }

        string? InputPattern { get; }
    }
}