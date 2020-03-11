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


            var rubyScriptProcessAssembly = Assembly.GetAssembly(typeof(RubyScriptProcess));
            Debug.Assert(rubyScriptProcessAssembly != null, nameof(rubyScriptProcessAssembly) + " != null");

            var processAssembly = Assembly.GetAssembly(typeof(Utilities.Processes.Process));
            Debug.Assert(processAssembly != null, nameof(processAssembly) + " != null");

            var nuixProcessSettings = new NuixProcessSettings(useDongle, nuixExeConsolePath);

            var methods = typeof(YamlRunner).GetMethods()
                .Where(m=>m.DeclaringType != typeof(object))
                    .Select(x=>x.AsRunnable(new YamlRunner(nuixProcessSettings), "Yaml"))
                .OfType<IDocumented>()
                .Concat(processAssembly.GetTypes()
                    .Where(t=> 
                        typeof(Utilities.Processes.Process).IsAssignableFrom(t))
                    .Where(t=>!t.IsAbstract)
                    .Select(x=> new YamlObjectWrapper(x,  "General Processes"))
                )
                .Concat(processAssembly.GetTypes()
                    .Where(t=> 
                        typeof(Utilities.Processes.enumerations.Enumeration).IsAssignableFrom(t))
                    .Where(t=>!t.IsAbstract)
                    .Select(x=> new YamlObjectWrapper(x,  "Enumerations"))
                )

                .Concat(processAssembly.GetTypes()
                    .Where(t=> 
                        typeof(Utilities.Processes.enumerations.Injection).IsAssignableFrom(t))
                    .Where(t=>!t.IsAbstract)
                    .Select(x=> new YamlObjectWrapper(x,  "Injections"))
                )

                    .Concat(rubyScriptProcessAssembly.GetTypes()
                        .Where(t=> typeof(RubyScriptProcess).IsAssignableFrom(t))
                        .Where(t=>!t.IsAbstract)
                        .Select(x=> new ProcessWrapper<INuixProcessSettings>(x, nuixProcessSettings, "Nuix Processes") )
                    )
                
                
                .ToList();

            ConsoleView.Run(args, methods);
        }
    }
}
