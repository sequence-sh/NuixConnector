using System.Collections.Generic;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// A block of ruby code that will be called by this script.
    /// </summary>
    public interface IRubyBlock
    {
        /// <summary>
        /// The name of the block.
        /// </summary>
        public string BlockName { get; }

        /// <summary>
        /// The functions that this block of code is dependent on.
        /// </summary>
        public IEnumerable<string> FunctionDefinitions { get; } //TODO use a class for this instead of a string

        /// <summary>
        /// Gets the arguments that need to be passed to the process.
        /// </summary>
        public IReadOnlyCollection<string> GetArguments(ref int blockNumber);

        /// <summary>
        /// Gets the lines needed in the options parser to provide the arguments.
        /// </summary>
        public IReadOnlyCollection<string> GetOptParseLines(ref int blockNumber);
    }
}