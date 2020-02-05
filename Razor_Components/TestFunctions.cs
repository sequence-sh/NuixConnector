using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Razor_Components
{
    /// <summary>
    /// Test functions for trying things out
    /// </summary>
    public static class TestFunctions
    {
        /// <summary>
        /// Returns the text every period
        /// </summary>
        /// <param name="cancellationToken">used to cancel</param>
        /// <param name="text">Text to return</param>
        /// <param name="period">Time in seconds</param>
        /// <returns></returns>
        public static IEnumerable<string> TestEnumerableResult(CancellationToken cancellationToken, string text = "Hello", int period = 1)
        {
            while (true)
            {
                Thread.Sleep(period * 1000);
                if (cancellationToken.IsCancellationRequested)
                    break;
                yield return text;
            }

            yield return "Cancelled";
        }


        /// <summary>
        /// Returns the text every period
        /// </summary>
        /// <param name="cancellationToken">used to cancel</param>
        /// <param name="text">Text to return</param>
        /// <param name="period">Time in seconds</param>
        /// <returns></returns>
        public static async IAsyncEnumerable<string> TestAsyncEnumerableResult(
            [EnumeratorCancellation]
            CancellationToken cancellationToken, string text = "Hello", int period = 1)
        {
            while (true)
            {
                await Task.Delay(period * 1000, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                    break;
                yield return text;
            }

            yield return "Cancelled";
        }

        /// <summary>
        /// Returns a range o integers
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public static List<int> TestIntResult(int min = 10, int max = 100)
        {
            return Enumerable.Range(10, max - min).ToList();
        }

        /// <summary>
        /// Concatenates two strings
        /// </summary>
        /// <param name="first">The first string</param>
        /// <param name="second">The second string</param>
        /// <returns></returns>
        public static string Concat(string first, string second)
        {
            return first + second;
        }

        /// <summary>
        /// Tasty things
        /// </summary>
        public enum  Fruit
        {
            /// <summary>
            /// Yummy and green
            /// </summary>
            Apple,
            /// <summary>
            /// Yummy and orange
            /// </summary>
            Orange,
            /// <summary>
            /// Yummy and yellow
            /// </summary>
            Banana
        }

        /// <summary>
        /// Turns an enum into a string
        /// </summary>
        /// <param name="fruit">The enum to turn into a string</param>
        /// <returns></returns>
        public static string EnumToString(Fruit fruit)
        {
            return fruit.ToString();
        }

        /// <summary>
        /// Looks at two booleans
        /// </summary>
        /// <param name="boolean">A boolean</param>
        /// <param name="nullableBoolean">A nullable boolean</param>
        /// <returns></returns>
        public static string TestBoolean(bool boolean, bool? nullableBoolean)
        {
            return $"b: {boolean}, nb: {nullableBoolean}";
        }

        /// <summary>
        /// Returns the text after the given amount of time
        /// </summary>
        /// <param name="text">Text to return</param>
        /// <param name="seconds">Amount of time to wait</param>
        /// <returns></returns>
        public static string VariableEcho(string text, int seconds)
        {
            Thread.Sleep(1000 * seconds);

            return text;
        }
        
        /// <summary>
        /// Returns the text after the given amount of time
        /// </summary>
        /// <param name="text">Text to return</param>
        /// <param name="seconds">Amount of time to wait</param>
        /// <returns></returns>
        public static async Task<string> VariableEchoAsync(string text, int seconds)
        {

            await Task.Run(()=>Thread.Sleep(1000 * seconds));

            return text;
        }



    }
}