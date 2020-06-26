using System.Configuration;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.mutable;
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
                var nuixProcesses = DynamicProcessFinder.GetAllDocumented(settings,
                    new DocumentationCategory("Nuix Processes", typeof(RubyScriptProcess)), typeof(RubyScriptProcess));

                var generalProcesses = DynamicProcessFinder.GetAllDocumented(settings,
                    new DocumentationCategory("General Processes", typeof(Process)), typeof(Process));

                var processes = nuixProcesses.Concat(generalProcesses);

                ConsoleView.Run(args, processes);
            }
            else
                foreach (var l in error.Split("\r\n"))
                    System.Console.WriteLine(l);
        }


    }
}
