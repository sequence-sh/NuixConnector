using System.Diagnostics;
using System.Linq;
using System.Reflection;
using InstantConsole;
using NuixClient.processes;
using Processes;

namespace NuixClientConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var rubyScriptProcessAssembly = Assembly.GetAssembly(typeof(RubyScriptProcess));
            Debug.Assert(rubyScriptProcessAssembly != null, nameof(rubyScriptProcessAssembly) + " != null");

            var methods = typeof(YamlRunner).GetMethods()
                    .Where(x=>x.IsStatic).Select(x=>x.AsRunnable())
                    .Concat(rubyScriptProcessAssembly.GetTypes()
                        .Where(t=> typeof(RubyScriptProcess).IsAssignableFrom(t))
                        .Select(x=> new NuixProcessWrapper(x) )
                    );

            ConsoleView.Run(args, methods);
        }
    }
}
