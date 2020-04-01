namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// A block of ruby code that creates a variable with a particular return type.
    /// </summary>
    public interface ITypedRubyBlock<T> : IRubyBlock
    {
        /// <summary>
        /// Gets the main block of ruby code.
        /// </summary>
        public string GetCallText(ref int blockNumber, out string resultVariableName);
    }
}