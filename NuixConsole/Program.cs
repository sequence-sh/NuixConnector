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
            string? yaml = null,
            [Option(LongName = "path", ShortName = "p", Description = "The path to the yaml to execute")]
            string? path = null) => ExecuteAbstract(yaml, path);

        [Command(Name = "documentation", Description = "Generate Documentation in Markdown format")]
        public void Documentation(
            [Option(LongName = "path", ShortName = "p", Description = "The path to the documentation file to write")]
            string path)
            => GenerateDocumentationAbstract(path);

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
        }




    }
}
