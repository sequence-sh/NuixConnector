using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Razor_Components
{
    internal static class ReflectionExtensions
    {
        public static object? InvokeWithNamedParameters(this MethodBase self, object obj, IReadOnlyDictionary<string, object?> namedParameters)
        {
            var mappedParameters = MapParameters(self, namedParameters);

            return self.Invoke(obj, mappedParameters);
        }

        public static object?[] MapParameters(MethodBase method, IReadOnlyDictionary<string, object?> namedParameters)
        {
            var paramNames = method.GetParameters().Select(p => p.Name).ToArray();
            var parameters = new object?[paramNames.Length];
            for (var i = 0; i < parameters.Length; ++i)
            {
                parameters[i] = Type.Missing;
            }
            foreach (var (paramName, value) in namedParameters)
            {
                var paramIndex = Array.IndexOf(paramNames, paramName);
                if (paramIndex >= 0)
                {
                    parameters[paramIndex] = value;
                }
            }
            return parameters;
        }
    }
}