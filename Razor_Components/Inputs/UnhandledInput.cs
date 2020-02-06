namespace Razor_Components.Inputs
{
    /// <summary>
    /// Don't know how to handle this input
    /// </summary>
    internal class UnhandledInput : IParameterInput
    {
        public UnhandledInput(string parameterName, string remarks, string error)
        {
            Error = error;
            ParameterName = parameterName;
            Remarks = remarks;
        }
        public string ParameterName { get; }
        public string Remarks { get; }

        public string Error { get; }
    }
}