namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// The parameter to a ruby method.
    /// </summary>
    public sealed class RubyMethodParameter
    {
        /// <summary>
        /// Creates a new RubyMethodParameter.
        /// </summary>
        public RubyMethodParameter(string parameterName, string? parameterValue, bool valueCanBeNull)
        {
            ParameterName = parameterName;
            ParameterValue = parameterValue;
            ValueCanBeNull = valueCanBeNull;
        }

        /// <summary>
        /// The name of the parameter.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// The value of the parameter, if it has been set.
        /// </summary>
        public string? ParameterValue { get; }

        /// <summary>
        /// Whether this parameter can be null. i.e. is it optional.
        /// </summary>
        public bool ValueCanBeNull { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"{ParameterName} = {ParameterValue}";
        }


        /// <summary>
        /// Deconstruct this RubyMethodParameter.
        /// </summary>
        /// <param name="parameterName"></param>
        /// <param name="parameterValue"></param>
        /// <param name="valueCanBeNull"></param>
        public void Deconstruct(out string parameterName, out string? parameterValue, out bool valueCanBeNull)
        {
            parameterName = ParameterName;
            parameterValue = ParameterValue;
            valueCanBeNull = ValueCanBeNull;
        }

    }
}