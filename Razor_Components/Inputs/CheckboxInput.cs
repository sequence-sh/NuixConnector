﻿namespace Razor_Components.Inputs
{
    internal class CheckboxInput : IParameterInput
    {
        public CheckboxInput(string parameterName, string remarks, bool nullable)
        {
            ParameterName = parameterName;
            Remarks = remarks;
            Nullable = nullable;
        }

        public string ParameterName { get; }
        public string Remarks { get; }
        public object? ConvertFromString(string s)
        {
            return bool.TryParse(s, out var t) ? t : null as bool?;
        }

        public bool Nullable { get; }
    }
}