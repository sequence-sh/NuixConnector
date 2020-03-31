using System.Collections.Generic;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// A call to a ruby method that returns a result of a particular type.
    /// </summary>
    public interface IMethodCall
    {
        /// <summary>
        /// The text of the method.
        /// </summary>
        public string MethodText { get; }

        /// <summary>
        /// The name of the method.
        /// </summary>
        public string MethodName { get; }

        /// <summary>
        /// Gets a line calling this method.
        /// </summary>
        /// <param name="methodNumber"></param>
        /// <returns></returns>
        public string GetMethodLine(int methodNumber);

        /// <summary>
        /// Gets the arguments that need to be passed to the method within the script.
        /// </summary>
        /// <param name="methodNumber"></param>
        /// <returns></returns>
        public IEnumerable<string> GetArguments(int methodNumber);

        /// <summary>
        /// Gets the lines needed in the options parser
        /// </summary>
        /// <param name="methodNumber"></param>
        /// <returns></returns>
        public IEnumerable<string> GetOptParseLines(int methodNumber);
    }

    /// <summary>
    /// A call to a ruby method that returns a result of a particular type.
    /// </summary>
    public interface IMethodCall<T> : IMethodCall
    {

    }
}