namespace Razor_Components.Inputs
{
    /// <summary>
    /// Don't need to display anything for this parameter - it will be handled elsewhere
    /// </summary>
    internal class NoInput : IParameterInput
    {
        public NoInput(string parameterName, string remarks)
        {
            ParameterName = parameterName;
            Remarks = remarks;
        }

        public string ParameterName { get; }
        public string Remarks { get; }
    }
}