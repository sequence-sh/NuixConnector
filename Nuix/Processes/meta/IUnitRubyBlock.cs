namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// A block of ruby code with no return type.
    /// </summary>
    public interface IUnitRubyBlock : IRubyBlock
    {
        /// <summary>
        /// Gets the main block of ruby code.
        /// </summary>
        public string GetBlockText(ref int blockNumber);
    }
}