using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;

namespace NuixClientConsole
{
    public interface IRunnable
    {
        string Name { get; }

        string Summary { get; }

        Result<Func<object?>, List<string>> TryGetInvocation(IReadOnlyDictionary<string, string> arguments);

        IEnumerable<IParameter> Parameters { get; }
    }
}