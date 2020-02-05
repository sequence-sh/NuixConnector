using System.Collections.Generic;

namespace Razor_Components
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

        public EnumConverter Converter { get; }

        public IReadOnlyCollection<string> Options { get; }

        public bool IsParameterNullable { get; }
    }
}