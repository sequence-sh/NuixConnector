namespace Razor_Components.Inputs
{
    /// <summary>
    /// A method parameter
    /// </summary>
    public interface IParameterInput
    {
        /// <summary>
        /// The name of the parameter
        /// </summary>
        string ParameterName { get; }

        /// <summary>
        /// XmlDoc comments for the parameter
        /// </summary>
        string Remarks { get; }
    }
}