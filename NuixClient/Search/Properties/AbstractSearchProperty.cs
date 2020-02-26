using JetBrains.Annotations;

namespace NuixClient.Search.Properties
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
        /// <param name="result">The result of rendering</param>
        /// <returns></returns>
        [ContractAnnotation("=>true,result:notNull; =>false,result:null;")]
        public abstract bool RenderValue(string t, out string? result);
    }
}