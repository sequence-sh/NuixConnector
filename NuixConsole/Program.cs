using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.mutable.enumerations;
using Reductech.EDR.Utilities.Processes.mutable.injection;
using Reductech.Utilities.InstantConsole;
using Process = Reductech.EDR.Utilities.Processes.mutable.Process;

namespace Reductech.EDR.Connectors.Nuix.Console
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            System.Console.OutputEncoding = Encoding.UTF8;

            var useDongleString =  ConfigurationManager.AppSettings["NuixUseDongle"];
            var nuixExeConsolePath = ConfigurationManager.AppSettings["NuixExeConsolePath"];
            var nuixVersionString = ConfigurationManager.AppSettings["NuixVersion"];
            var nuixFeaturesString = ConfigurationManager.AppSettings["NuixFeatures"];

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

            if (!Version.TryParse(nuixVersionString, out var nuixVersion))
            {
                System.Console.WriteLine("Please set the property 'NuixVersion' in the settings file to a valid version number");
                return;
            }

            if (!TryParseNuixFeatures(nuixFeaturesString, out var nuixFeatures))
            {
                System.Console.WriteLine("Please set the property 'NuixFeatures' in the settings file to a comma separated list of nuix features or 'NO_FEATURES'");
                return;
            }


            var rubyScriptProcessAssembly = Assembly.GetAssembly(typeof(RubyScriptProcess));
            Debug.Assert(rubyScriptProcessAssembly != null, nameof(rubyScriptProcessAssembly) + " != null");

            var processAssembly = Assembly.GetAssembly(typeof(Process));
            Debug.Assert(processAssembly != null, nameof(processAssembly) + " != null");

            var nuixProcessSettings = new NuixProcessSettings(useDongle, nuixExeConsolePath, nuixVersion, nuixFeatures);

            var methods = typeof(YamlRunner).GetMethods()
                .Where(m=>m.DeclaringType != typeof(object))
                    .Select(x=>x.AsRunnable(new YamlRunner(nuixProcessSettings), new DocumentationCategory("Yaml")))
                .OfType<IDocumented>()
                .OrderBy(x=>x.Name)
                .Concat(processAssembly.GetTypes()
                    .Where(t=> 
                        typeof(Process).IsAssignableFrom(t)
                        
                        )
                    .Where(t=>!t.IsAbstract)
                    .Select(x=> new YamlObjectWrapper(x, new DocumentationCategory("General Processes", typeof(Process)))

                ).OrderBy(x=>x.Name))
                .Concat(processAssembly.GetTypes()
                    .Where(t=> 
                        typeof(Enumeration).IsAssignableFrom(t) || typeof(Injection).IsAssignableFrom(t))
                    .Where(t=>!t.IsAbstract)
                    .Select(x=> new YamlObjectWrapper(x, new DocumentationCategory("Enumerations", typeof(Enumeration)))
                    ).OrderBy(x=>x.Name))
                

                    .Concat(rubyScriptProcessAssembly.GetTypes()
                        .Where(t=> typeof(RubyScriptProcess).IsAssignableFrom(t))
                        .Where(t=>!t.IsAbstract)
                        .Select(x=> new ProcessWrapper<INuixProcessSettings>(x, nuixProcessSettings, new DocumentationCategory("Nuix Processes"))
                        ).OrderBy(x=>x.Name))

                .ToList();

            ConsoleView.Run(args, methods);
        }

        private static bool TryParseNuixFeatures(string? s, out IReadOnlyCollection<NuixFeature> nuixFeatures)
        {
            if(string.IsNullOrWhiteSpace(s))
            {
                nuixFeatures = new List<NuixFeature>();
                return false;
            }
            else if (s == "NO_FEATURES")
            {
                nuixFeatures = new List<NuixFeature>();
                return true;
            }
            else
            {
                var nfs = new HashSet<NuixFeature>();
                var features = s.Split(',');
                foreach (var feature in features)
                    if (Enum.TryParse(typeof(NuixFeature), feature, true, out var nf) && nf is NuixFeature nuixFeature)
                        nfs.Add(nuixFeature);

                nuixFeatures = nfs;
                return true;
            }
        }
        
    }
}
