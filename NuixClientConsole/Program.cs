using System;
using System.Linq;

namespace NuixClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var methods = 
                typeof(NuixClient.OutsideScripting).GetMethods()
                    .Concat(typeof(NuixClient.ProcessRunner).GetMethods())
                    .Where(x=>x.IsStatic);

            var lines = ConsoleView.Run(args, methods);

            var enumerator = lines.GetAsyncEnumerator();

            try
            {
                while (enumerator.MoveNextAsync().Result)
                {
                    Console.WriteLine(enumerator.Current);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }
}
