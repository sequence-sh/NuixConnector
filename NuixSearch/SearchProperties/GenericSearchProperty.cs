using System.Diagnostics;
using CSharpFunctionalExtensions;

namespace NuixSearch.SearchProperties
{
    /// <summary>
    /// A property that one could search by
    /// </summary>
    /// <typeparam name="T">The type of the property</typeparam>
    internal abstract class GenericSearchProperty<T> : AbstractSearchProperty
    {
        /// <summary>
        /// Create a new search property
        /// </summary>
        /// <param name="propertyName"></param>
        protected GenericSearchProperty(string propertyName) : base(propertyName)
        {
        }

        /// <summary>
        /// TryParse a string as an object
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        protected abstract Result<T> TryParse(string s);


        protected virtual string ConvertToString(T t)
        {
            Debug.Assert(t != null, nameof(t) + " != null");
            var r = t.ToString();

            Debug.Assert(r != null, nameof(r) + " != null");
            return r;
        }

        /// <summary>
        /// Render this property with this value
        /// </summary>
        /// <returns></returns>
        public override Result<string> TryRender(string str)
        {
            var r = TryParse(str);

            return r.Map(ConvertToString);
        }
    }
}