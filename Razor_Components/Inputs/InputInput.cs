namespace Razor_Components.Inputs
{
    /// <summary>
    /// Input a text area
    /// </summary>
    internal class TextAreaInput : IParameterInput
    {
        public TextAreaInput(string parameterName, string remarks, IConverter converter)
        {
            ParameterName = parameterName;
            Remarks = remarks;
            Converter = converter;
        }

        public string ParameterName { get; }
        public string Remarks { get; }

        public IConverter Converter { get; }
    }


    /// <summary>
    /// Input will be handled by an input control
    /// </summary>
    internal class InputInput : IParameterInput
    {
        public InputInput(string parameterName, string remarks, IConverter converter, bool nullable)
        {
            ParameterName = parameterName;
            Remarks = remarks;
            Converter = converter;
            Nullable = nullable;
        }

        public string ParameterName { get; }
        public string Remarks { get; }

        public IConverter Converter { get; }

        public bool Nullable { get; }
    }
}