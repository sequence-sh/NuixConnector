using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Orchestration.Conditions;

namespace Orchestration
{
    /// <summary>
    /// Runs processes from Yaml
    /// </summary>
    public static class YamlRunner
    {
        /// <summary>
        /// Run process defined in yaml
        /// </summary>
        /// <param name="yamlString">Yaml representing the process</param>
        /// <returns></returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<Result<string>> RunProcessFromYamlString(string yamlString)
        {
            var yamlResult = YamlHelper.TryMakeFromYaml(yamlString);

            if (yamlResult.IsFailure)
            {
                yield return yamlResult.ConvertFailure<string>();
                yield break;
            }

            foreach (var processCondition in yamlResult.Value.Conditions ?? Enumerable.Empty<Condition>())
            {
                if (processCondition.IsMet())
                    yield return Result.Success(processCondition.GetDescription());
                else
                {
                    yield return Result.Failure<string>($"CONDITION NOT MET: [{processCondition.GetDescription()}]");
                    yield break;
                }
            }

            var resultLines = yamlResult.Value.Execute();
            await foreach (var resultLine in resultLines)
            {
                yield return resultLine;
            }
        }

        /// <summary>
        /// Run process defined in yaml found at a particular path
        /// </summary>
        /// <param name="yamlPath">Path to the yaml</param>
        /// <returns></returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<Result<string>> RunProcessFromYaml(string yamlPath)
        {
            string? text;
            string? errorMessage;
            try
            {
                text = File.ReadAllText(yamlPath);
                errorMessage = null;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                errorMessage = e.Message;
                text = null;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            if (errorMessage != null)
            {
                yield return Result.Failure<string>(errorMessage);
            }
            else if (!string.IsNullOrWhiteSpace(text))
            {
                var r = RunProcessFromYamlString(text);
                await foreach(var rl in r)
                    yield return rl;
            }
            else
            {
                yield return Result.Failure<string>("File is empty");
            }
        }

    }
}
