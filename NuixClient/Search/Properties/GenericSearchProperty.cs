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


        protected virtual string? ConvertToString(T t)
        {
            var r = t?.ToString();

            return r;
        }

        /// <summary>
        /// Render this property with this value
        /// </summary>
        /// <returns></returns>
        public override Result<string> TryRender(string str)
        {
            var r = TryParse(str);

            if (r is Success<T> s)
            {
                var renderedString = ConvertToString(s.Result);

                if (renderedString == null)
                    return Result<string>.Failure($"Could not render '{str}'");

                return Result<string>.Success(renderedString);
            }
            else
                return Result<string>.Failure(r.Errors);
        }
    }
}