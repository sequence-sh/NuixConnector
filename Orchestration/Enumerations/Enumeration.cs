using System.Collections.Generic;

namespace Orchestration.Enumerations
{
    /// <summary>
    /// Represents a list of elements
    /// </summary>
    public abstract class Enumeration
    {
        internal abstract IEnumerable<string> Elements { get; }
        internal abstract string Name { get; }


        /// <inheritdoc />
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            return obj is Enumeration e && e.Name == Name && GetType() == e.GetType();
        }
        
        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}