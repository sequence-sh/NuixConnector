using System.Collections;
using System.Collections.Generic;
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
}