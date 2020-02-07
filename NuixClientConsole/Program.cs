using System;
using System.Linq;

namespace NuixClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var methods = typeof(NuixClient.OutsideScripting).GetMethods().Where(x=>x.IsStatic);

            var lines = ConsoleView.Run(args, methods);

            var enumerator = lines.GetAsyncEnumerator();

            try
            {
                while (enumerator.MoveNextAsync().Result)
                {
                    Console.WriteLine(enumerator.Current);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
