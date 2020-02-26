using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace NuixClient.Search
{
    /// <summary>
    /// A property that one could search by
    /// </summary>
    /// <typeparam name="T">The type of the property</typeparam>
    public class SearchProperty<T> : SearchProperty
    {
        /// <summary>
        /// Create a new search property
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="parseFunc"></param>
        internal SearchProperty(string propertyName, Func<string,(bool success, T obj)> parseFunc) : base(propertyName)
        {
            _parseFunc = parseFunc;
        }

        private readonly Func<string, (bool success, T obj)> _parseFunc;

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
        public override bool RenderValue(string str,out string? result)
        {
            var (success, obj) = _parseFunc(str);

            if (success)
            {
                result = RenderValue(obj);
                return true;
            }
            else
            {
                result = null;
                return false;
            }
        }
    }

    /// <summary>
    /// A property that one could search by
    /// </summary>
    public abstract class SearchProperty
    {
        internal SearchProperty(string propertyName)
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