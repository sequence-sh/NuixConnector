using CSharpFunctionalExtensions;

namespace Reductech.EDR.Connectors.Nuix.Search.SearchProperties
{
    /// <summary>
    /// A property that one could search by
    /// </summary>
    internal abstract class AbstractSearchProperty
    {
        internal AbstractSearchProperty(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// The name of this property
        /// </summary>
        public readonly string PropertyName;

        /// <summary>
        /// String representation of this SearchProperty
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return PropertyName;
        }

        /// <summary>
        /// Render a property with this value. Assumes T has the correct type
        /// </summary>
        /// <param name="t">The value of the property. Must have the correct type</param>
        /// <returns></returns>
        public abstract Result<string> TryRender(string t);
    }
}