﻿using System;
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

        public static string CompileScriptSetup(string name, IEnumerable<IMethodCall> methodCalls)
        {
            var scriptStringBuilder = new StringBuilder();

            scriptStringBuilder.AppendLine("require 'optparse'");
            scriptStringBuilder.AppendLine($"#{name}");

            //Parse options
            scriptStringBuilder.AppendLine(HashSetName + " = {}");
            scriptStringBuilder.AppendLine("OptionParser.new do |opts|");

            foreach (var extraOptParseLine in methodCalls.SelectMany((x, i) => x.GetOptParseLines(i)))
                scriptStringBuilder.AppendLine(extraOptParseLine);
            scriptStringBuilder.AppendLine($"end.parse!(into: {HashSetName})");

            // ReSharper disable once JoinDeclarationAndInitializer
            bool printArguments;
#if DEBUG 
            printArguments = true;
#endif
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if(printArguments)
                scriptStringBuilder.AppendLine($"puts {HashSetName}");

            return scriptStringBuilder.ToString();
        }


        public static string CompileScriptMethodText(IReadOnlyCollection<IMethodCall> methodCalls)
        {
            var methodsTextStringBuilder = new StringBuilder();

            foreach (var method in methodCalls.Select(x=>x.MethodText).Distinct())
            {
                methodsTextStringBuilder.AppendLine(method);
                methodsTextStringBuilder.AppendLine();
            }

            return methodsTextStringBuilder.ToString();
        }

        public static async Task<List<string>> GetTrueArgumentsAsync(string scriptText, INuixProcessSettings nuixProcessSettings, IEnumerable<IMethodCall> methodCalls)
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
            trueArguments.AddRange(methodCalls.SelectMany((x,i)=> x.GetArguments(i)));

            return trueArguments;
        }
    }
}