using System;
using System.Collections.Generic;
using System.Configuration;
using CommandDotNet;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.Console
{
    [Command(Description = "Executes Nuix processes")]

    internal class NuixMethods : ProcessMethods
    {
        [Command(Name = "execute", Description = "Execute a process defined in yaml")]
        public void Execute(
            [Option(LongName = "yaml", ShortName = "y", Description = "The yaml to execute")]
            string? yaml,
            [Option(LongName = "path", ShortName = "p", Description = "The path to the yaml to execute")]
            string? path) => ExecuteAbstract(yaml, path);

        [Command(Name = "documentation", Description = "Generate Documentation in Markdown format")]
        public void Documentation() => GenerateDocumentationAbstract();

        [Command(Name= "generatescripts", Description = "Generates Ruby Scripts for Nuix processes")]
        public void GenerateScripts(
            [Option(LongName = "folderPath", ShortName = "f", Description = "The path to the output folder")]
            string folderPath)
        {
            var settingsResult = NuixProcessSettings
                .TryCreate(sn => ConfigurationManager.AppSettings[sn]);
            if (settingsResult.IsFailure)
                throw new Exception(settingsResult.Error);

            var generator = new ScriptGenerator(settingsResult.Value);

            var r = generator.GenerateScripts(folderPath);

            Logger.LogInformation(r);
        }

        /// <inheritdoc />
        protected override Result<IProcessSettings> TryGetSettings()
        {
            var settingsResult = NuixProcessSettings
                .TryCreate(sn => ConfigurationManager.AppSettings[sn])
                .Map(x=>x as IProcessSettings);

            return settingsResult;
        }

        /// <inheritdoc />
        protected override IEnumerable<Type> ConnectorTypes { get; } = new List<Type>(){typeof(IRubyScriptProcess)};

        /// <inheritdoc />
        protected override ILogger Logger { get; } =
            new ServiceCollection().AddLogging(cfg => cfg.AddConsole()).BuildServiceProvider().GetService<ILogger<NuixMethods>>();
    }


    internal class Program
    {
        private static void Main(string[] args)
        {
            var appRunner = new AppRunner<NuixMethods>().UseDefaultMiddleware();

            appRunner.Run(args);

            //System.Console.OutputEncoding = Encoding.UTF8;

            //var (isSuccess, _, settings, error)  = NuixProcessSettings.TryCreate(sn => ConfigurationManager.AppSettings[sn]);


            //if (isSuccess)
            //{
            //    // instantiate DI and configure logger
            //    var serviceProvider = new ServiceCollection().AddLogging(cfg => cfg.AddConsole()).BuildServiceProvider();
            //    // get instance of logger
            //    var logger = serviceProvider.GetService<ILogger<Process>>();


            //    var nuixProcesses = DynamicProcessFinder.GetAllDocumented(settings,
            //        new DocumentationCategory("Nuix Processes", typeof(RubyScriptProcessUnit)), typeof(RubyScriptProcessUnit), logger);

            //    var generalProcesses = DynamicProcessFinder.GetAllDocumented(settings,
            //        new DocumentationCategory("General Processes", typeof(Process)), typeof(Process), logger);


            //    var scriptGenerator = new ScriptGenerator(settings);

            //    var generateScriptsMethod = typeof(ScriptGenerator).GetMethod(nameof(scriptGenerator.GenerateScripts))!;

            //    var generateScriptsMethodWrapper = new MethodWrapper(generateScriptsMethod, scriptGenerator, new DocumentationCategory("Nuix Meta"));

            //    var processes = nuixProcesses.Concat(generalProcesses).Prepend(generateScriptsMethodWrapper);

            //    ConsoleView.Run(args, processes);
            //}
            //else
            //    foreach (var l in error.Split("\r\n"))
            //        System.Console.WriteLine(l);
        }




    }
}
