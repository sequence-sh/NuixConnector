using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace InstantConsole
{
    /// <summary>
    /// A method that can be run by the console app.
    /// </summary>
    public interface IRunnable
    {
        /// <summary>
        /// The name of the method.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// A summary of what the method does.
        /// </summary>
        string Summary { get; }

        /// <summary>
        /// Uses the parameter arguments to get a function which runs the method with those arguments.
        /// If this fails it should return a list of the different problems.
        /// </summary>
        /// <param name="arguments"></param>
        /// <returns></returns>
        Result<Func<object?>, List<string?[]>> TryGetInvocation(IReadOnlyDictionary<string, string> arguments);

        /// <summary>
        /// The parameters to the method.
        /// </summary>
        IEnumerable<IParameter> Parameters { get; }
    }
}