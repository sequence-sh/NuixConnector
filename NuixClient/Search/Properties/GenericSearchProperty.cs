using System.Diagnostics;
using Orchestration;

namespace NuixClient.Search.Properties
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

        /// <summary>
        /// Render this property with this value
        /// </summary>
        /// <returns></returns>
        public string RenderValue(T t)
        {

            var tString = t is string s && s.Contains(" ") ? $"\"{s}\"" : t?.ToString();

            Debug.Assert(tString != null, nameof(tString) + " != null");
            return tString;
        }

        /// <summary>
        /// Render this property with this value
        /// </summary>
        /// <returns></returns>
        public override bool RenderValue(string str, out string? result)
        {
            var r = TryParse(str);

            if (r is Success<T> s)
            {
                result = RenderValue(s.Result);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }
}