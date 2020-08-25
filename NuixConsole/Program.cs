using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Processes.Internal;
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
                // instantiate DI and configure logger
                var serviceProvider = new ServiceCollection().AddLogging(cfg => cfg.AddConsole()).BuildServiceProvider();
                // get instance of logger
                var logger = serviceProvider.GetService<ILogger<Process>>();


                var nuixProcesses = DynamicProcessFinder.GetAllDocumented(settings,
                    new DocumentationCategory("Nuix Processes", typeof(RubyScriptProcess)), typeof(RubyScriptProcess), logger);

                var generalProcesses = DynamicProcessFinder.GetAllDocumented(settings,
                    new DocumentationCategory("General Processes", typeof(Process)), typeof(Process), logger);

                var processes = nuixProcesses.Concat(generalProcesses);

                ConsoleView.Run(args, processes);
            }
            else
                foreach (var l in error.Split("\r\n"))
                    System.Console.WriteLine(l);
        }


    }
}
