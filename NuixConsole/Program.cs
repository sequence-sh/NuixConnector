using System.Configuration;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.processes;
using Reductech.EDR.Utilities.Processes;
using Reductech.Utilities.InstantConsole;

namespace Reductech.EDR.Connectors.Nuix.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            System.Console.OutputEncoding = Encoding.UTF8;

            var (isSuccess, _, settings, error)  = NuixProcessSettings.TryCreate(sn => ConfigurationManager.AppSettings[sn]);


            if (isSuccess)
            {
                var processes =
                    AllProcesses.GetProcesses(settings)
                        .Concat(AllProcesses.EnumerationWrappers)
                        .Concat(AllProcesses.InjectionWrappers)
                        .Concat(NuixProcesses.GetProcesses(settings)).ToList();

                ConsoleView.Run(args, processes);
            }
            else
                foreach (var l in error.Split("\r\n"))
                    System.Console.WriteLine(l);
        }


    }
}
