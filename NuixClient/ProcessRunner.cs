﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NuixClient.Orchestration;

namespace NuixClient
{
    /// <summary>
    /// Runs processes from Json
    /// </summary>
    public static class ProcessRunner
    {
        /// <summary>
        /// Run process defined in Json
        /// </summary>
        /// <param name="jsonPath">Path to the JSON</param>
        /// <returns></returns>
        public static async IAsyncEnumerable<ResultLine> RunProcessFromJson(string jsonPath)
        {

            Process? process = null;

            ResultLine? errorLine;

            try
            {
                var text = File.ReadAllText(jsonPath);

                process = JsonConvert.DeserializeObject<Process>(text,
                    new ProcessJsonConverter(),
                    new ConditionJsonConverter());
                errorLine = null;
            }
            catch (Exception e)
            {
                errorLine = new ResultLine(false, e.Message);
            }

            if (errorLine != null)
            {
                yield return errorLine;
            }
            else if(process == null)
            {
                yield return  new ResultLine(false, "Process is null");
            }
            else
            {
                foreach (var processCondition in process.Conditions ?? Enumerable.Empty<Condition>())
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

    }
}
