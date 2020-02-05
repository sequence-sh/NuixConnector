namespace Razor_Components
{
    /// <summary>
    /// Input will be handled by an input control
    /// </summary>
    internal class InputInput : IParameterInput
    {
        public InputInput(string parameterName, string remarks, IConverter converter,  bool nullable)
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