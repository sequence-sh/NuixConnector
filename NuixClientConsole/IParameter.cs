using System;

namespace NuixClientConsole
{
    public interface IParameter
    {
        string Name { get; }

        string Summary { get; }

        Type Type { get; }

    }
}