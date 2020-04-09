using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    internal static class RubyScriptCompilationHelper
    {
        /// <summary>
        /// The name of the parameters hash set
        /// </summary>
        public const string HashSetName = "params";

        public static string CompileScriptSetup(string name, IReadOnlyCollection<IRubyBlock> rubyBlocks)
        {
            var scriptStringBuilder = new StringBuilder();

            scriptStringBuilder.AppendLine($"#{name}");
            scriptStringBuilder.AppendLine();

            var highestVersion =
                rubyBlocks.Select(x => x.RequiredNuixVersion).OrderByDescending(x => x).FirstOrDefault();

            if (highestVersion != null)
            {
                scriptStringBuilder.AppendLine($"requiredNuixVersion = '{highestVersion}'");
                scriptStringBuilder.AppendLine("if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)");
                scriptStringBuilder.AppendLine("\tputs \"Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required\"");
                scriptStringBuilder.AppendLine("\texit");
                scriptStringBuilder.AppendLine("end");
                scriptStringBuilder.AppendLine();
            }

            var requiredFeatures = rubyBlocks.SelectMany(x => x.RequiredNuixFeatures).Distinct().OrderBy(x=>x).ToList();
            if (requiredFeatures.Any())
            {
                var featuresArray = string.Join(", ", requiredFeatures.Select(rf => $"'{rf}'"));
                scriptStringBuilder.AppendLine($"requiredFeatures = Array[{featuresArray}]");
                scriptStringBuilder.AppendLine("requiredFeatures.each do |feature|");
                scriptStringBuilder.AppendLine("\tif !utilities.getLicence().hasFeature(feature)");
                scriptStringBuilder.AppendLine("\t\tputs \"Nuix Feature #{feature} is required but not available.\"");
                scriptStringBuilder.AppendLine("\t\texit");
                scriptStringBuilder.AppendLine("\tend");
                scriptStringBuilder.AppendLine("end");
                scriptStringBuilder.AppendLine();
            }

            scriptStringBuilder.AppendLine("require 'optparse'");

            //Parse options
            scriptStringBuilder.AppendLine(HashSetName + " = {}");
            scriptStringBuilder.AppendLine("OptionParser.new do |opts|");

            var i = 0;
            foreach (var methodCall in rubyBlocks)
            {
                var optParseLines = methodCall.GetOptParseLines(HashSetName, ref i);
                foreach (var optParseLine in optParseLines) scriptStringBuilder.AppendLine('\t' + optParseLine);
            }

            scriptStringBuilder.AppendLine("end.parse!");
            scriptStringBuilder.AppendLine();

            // ReSharper disable once JoinDeclarationAndInitializer
            bool printArguments;
#if DEBUG 
            printArguments = true;
#endif
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if (printArguments)
            {
                scriptStringBuilder.AppendLine($"puts {HashSetName}");
                scriptStringBuilder.AppendLine();
            }
                

            return scriptStringBuilder.ToString();
        }


        public static string CompileScriptMethodText(IReadOnlyCollection<IRubyBlock> rubyBlocks)
        {
            var methodsTextStringBuilder = new StringBuilder();

            foreach (var method in rubyBlocks.SelectMany(x=>x.FunctionDefinitions).Distinct())
            {
                methodsTextStringBuilder.AppendLine(method);
                methodsTextStringBuilder.AppendLine();
            }

            return methodsTextStringBuilder.ToString();
        }

        public static async Task<List<string>> GetTrueArgumentsAsync(string scriptText, INuixProcessSettings nuixProcessSettings, IEnumerable<IRubyBlock> rubyBlocks)
        {
            
            var scriptFilePath = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), "NuixScript" + Guid.NewGuid()), "rb");
            
            await File.WriteAllTextAsync(scriptFilePath, scriptText);

            var trueArguments = new List<string>(); //note that the arguments will be escaped in the next step
            if (nuixProcessSettings.UseDongle)
            {
                // ReSharper disable once StringLiteralTypo
                trueArguments.Add("-licencesourcetype");
                trueArguments.Add("dongle");  
            }
            trueArguments.Add(scriptFilePath);
            var i = 0;
            foreach (var methodCall in rubyBlocks)
            {
                var arguments = methodCall.GetArguments(ref i);
                trueArguments.AddRange(arguments);
            }

            return trueArguments;
        }

        /// <summary>
        /// Gets a string which will point to the argument value in ruby;
        /// </summary>
        public static string GetArgumentValueString(string argumentName)
        {
            return $"{HashSetName}[:{argumentName}]";
        }

        public static readonly ISet<string> NuixWarnings = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "ERROR StatusLogger Log4j2 could not find a logging implementation. Please add log4j-core to the classpath. Using SimpleLogger to log to the console..."
        };
    }
}