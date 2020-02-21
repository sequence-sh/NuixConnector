using System.Collections.Generic;

namespace NuixClient.Orchestration.Enumerations
{
    /// <summary>
    /// Represents a list of elements
    /// </summary>
    internal abstract class Enumeration
    {
        internal abstract IEnumerable<string> Elements { get; }
        internal abstract string Name { get; }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object? obj)
        {
            return obj is Enumeration e && e.Name == Name && GetType() == e.GetType();
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}