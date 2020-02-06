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

        /// <summary>
        /// Converts a string (from the user metadata store) into the default value for this parameter
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        object? ConvertFromString(string s);
    }
}