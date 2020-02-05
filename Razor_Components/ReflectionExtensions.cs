using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Razor_Components
{
    internal static class EnumerableExtensions
    {
        public static async IAsyncEnumerable<T> AsAsyncEnumerable<T>(
            IEnumerable<T> enumerable,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using var enumerator = enumerable.GetEnumerator();

            while (true)
            {
                var hadNext = await Task.Run(()=> enumerator.MoveNext(), cancellationToken);
                if (!hadNext) break;
                var item = enumerator.Current;
                yield return item;

            }
        }
        
        
        public static async IAsyncEnumerable<object?> AsAsyncEnumerable(
            IEnumerable enumerable,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var enumerator = enumerable.GetEnumerator();

            while (true)
            {
                var hadNext = await Task.Run(()=> enumerator.MoveNext(), cancellationToken);
                if (!hadNext) break;
                var item = enumerator.Current;
                yield return item;

            }
        }
    }

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