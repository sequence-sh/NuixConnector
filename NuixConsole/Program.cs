using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
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

            var rubyScriptProcessAssembly = Assembly.GetAssembly(typeof(RubyScriptProcess));
            Debug.Assert(rubyScriptProcessAssembly != null, nameof(rubyScriptProcessAssembly) + " != null");

            var useDongleString =  ConfigurationManager.AppSettings["NuixUseDongle"];
            var nuixExeConsolePath = ConfigurationManager.AppSettings["NuixExeConsolePath"];

            if (!bool.TryParse(useDongleString, out var useDongle))
            {
                System.Console.WriteLine("Please set the property 'NuixUseDongle' in the settings file");
                return;
            }

            if (string.IsNullOrWhiteSpace(nuixExeConsolePath))
            {
                System.Console.WriteLine("Please set the property 'NuixExeConsolePath' in the settings file");
                return;
            }

            var nuixProcessSettings = new NuixProcessSettings(useDongle, nuixExeConsolePath);

            var methods = typeof(YamlRunner).GetMethods()
                .Where(m=>m.DeclaringType != typeof(object))
                    .Select(x=>x.AsRunnable(new YamlRunner(nuixProcessSettings)))
                    .Concat(rubyScriptProcessAssembly.GetTypes()
                        .Where(t=> typeof(RubyScriptProcess).IsAssignableFrom(t))
                        .Where(t=>!t.IsAbstract)
                        .Select(x=> new NuixProcessWrapper(x, nuixProcessSettings) )
                    ).ToList();

            ConsoleView.Run(args, methods);
        }
    }
}
