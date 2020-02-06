﻿using System.Collections.Generic;

namespace Razor_Components.Inputs
{
    internal class DropdownInput : IParameterInput
    {
        public DropdownInput(string parameterName, string remarks, EnumConverter converter, IReadOnlyCollection<string> options, bool isParameterNullable)
        {
            ParameterName = parameterName;
            Remarks = remarks;
            Converter = converter;
            Options = options;
            IsParameterNullable = isParameterNullable;
        }

        public string ParameterName { get; }
        public string Remarks { get; }
        public object? ConvertFromString(string s)
        {
            var (success, result) = Converter.FromString(s);
            return success ? result : null;
        }

        public EnumConverter Converter { get; }

        public IReadOnlyCollection<string> Options { get; }

        public bool IsParameterNullable { get; }
    }
}