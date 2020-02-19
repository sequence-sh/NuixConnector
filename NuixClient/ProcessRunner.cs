using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using NuixClient.Orchestration;

namespace NuixClient
{
    /// <summary>
    /// Runs processes from Json
    /// </summary>
    public static class ProcessRunner
    {
        /// <summary>
        /// Run process defined in yaml
        /// </summary>
        /// <param name="yamlString">Yaml representing the process</param>
        /// <returns></returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> RunProcessFromYamlString(string yamlString)
        {

            if (!YamlHelper.TryMakeFromYaml(yamlString, out var process, out var e))
            {
#pragma warning disable CS8604 // Possible null reference argument.
                yield return new ResultLine(false, e);
#pragma warning restore CS8604 // Possible null reference argument.
            }
            else
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                foreach (var processCondition in process.Conditions ?? Enumerable.Empty<Condition>())
#pragma warning restore CS8602 // Dereference of a possibly null reference.
                {
                    if (processCondition.IsMet())
                        yield return new ResultLine(true, processCondition.GetDescription());
                    else
                    {
                        yield return new ResultLine(false, $"CONDITION NOT MET: [{processCondition.GetDescription()}]");
                        yield break;
                    }
                }

                var resultLines = process.Execute();
                await foreach (var resultLine in resultLines)
                {
                    yield return resultLine;
                }
            }
        }

        /// <summary>
        /// Run process defined in yaml found at a particular path
        /// </summary>
        /// <param name="yamlPath">Path to the yaml</param>
        /// <returns></returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> RunProcessFromYaml(string yamlPath)
        {
            string? text;
            ResultLine? errorLine = null;
            try
            {
                text = File.ReadAllText(yamlPath);                
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                errorLine = new ResultLine(false, e.Message);
                text = null;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            if (errorLine != null)
            {
                yield return errorLine;
            }
            else if (!string.IsNullOrWhiteSpace(text))
            {
                var r = RunProcessFromYamlString(text);
                await foreach(var rl in r)
                {
                    yield return rl;
                }
            }
            else
            {
                yield return new ResultLine(false, "File is empty");
            }
        }

    }
}
