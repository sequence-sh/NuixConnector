using System;
using System.Linq;

namespace InstantConsole
{
    /// <summary>
    /// The parameter to a runnable method.
    /// </summary>
    public interface IParameter
    {
        /// <summary>
        /// The name of the parameter.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// /// A summary of what this parameter does.
        /// </summary>
        string Summary { get; }

        /// <summary>
        /// The type of the parameter.
        /// </summary>
        Type Type { get; }

        /// <summary>
        /// Human readable name of the parameter type.
        /// </summary>
        public string TypeName => GetFullName(Type);

        /// <summary>
        /// Is this parameter required.
        /// </summary>
        bool Required { get; }


        private static string GetFullName(Type t)
        {
            if (!t.IsGenericType)
                return t.Name;

            var typeName = t.Name.Split("`")[0];

            var arguments = $"<{string.Join(",", t.GetGenericArguments().Select(GetFullName))}>";

            return typeName + arguments;
        }

    }
}