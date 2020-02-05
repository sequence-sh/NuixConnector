namespace Razor_Components
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

        public bool Nullable { get; }
    }
}