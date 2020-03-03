using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using YamlDotNet.Serialization;

namespace Orchestration.Processes
{
    /// <summary>
    /// Runs an external process
    /// </summary>
    public class RunExternalProcess : Process
    {
        /// <summary>
        /// The path to the process to run
        /// </summary>
        [YamlMember(Order = 2)]
        [Required]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string ProcessPath { get; set; }

        /// <summary>
        /// Pairs of parameters to give to the process
        /// </summary>
        [YamlMember(Order = 3)]
        [Required]
        public Dictionary<string, string> Parameters { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

        /// <summary>
        /// The name of an additional parameter.
        /// This is intended for use with injection.
        /// </summary>
        [YamlMember(Order = 4)]
        public string? ExtraParameterName { get; set; }

        /// <summary>
        /// The value of the additional parameter.
        /// This is intended for use with injection.
        /// </summary>
        [YamlMember(Order = 5)]
        public string? ExtraParameterValue { get; set; }

        /// <inheritdoc />
        public override IEnumerable<string> GetArgumentErrors()
        {
            if (ProcessPath == null)
                yield return $"{nameof(ProcessPath)} must not be null.";
            else if (!ProcessPath.EndsWith(".exe"))
                yield return $"'{ProcessPath}' does not point to an executable file.";
            if (!File.Exists(ProcessPath))
                yield return $"'{ProcessPath}' does not exist.";

            if (ExtraParameterName != null && ExtraParameterValue == null)
                yield return $"{nameof(ExtraParameterValue)} is null.";
            if (ExtraParameterName == null && ExtraParameterValue != null)
                yield return $"{nameof(ExtraParameterName)} is null.";
        }

        /// <inheritdoc />
        public override string GetName()
        {
            return $"{ProcessPath} {string.Join(" ", Parameters.Select((k, v) => $"-{k} {v}"))}" +
                   (ExtraParameterName != null && ExtraParameterValue != null
                       ? $" -{ExtraParameterName} {ExtraParameterValue}"
                       : "");
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute()
        {
            var argumentErrors = GetArgumentErrors().ToList();

            if (argumentErrors.Any())
            {
                foreach (var ae in argumentErrors)
                    yield return Result.Failure<string>(ae);
                yield break;
            }

            var args = new List<string>();

            foreach (var (key, value) in Parameters)
            {
                args.Add($"-{key}");
                args.Add(value);
            }

            if (ExtraParameterName != null && ExtraParameterValue != null)
            {
                args.Add($"-{ExtraParameterName}");
                args.Add(ExtraParameterValue);
            }
            var result = ExternalProcessHelper.RunExternalProcess(ProcessPath, args);

            await foreach (var line in result)
            {
                    yield return line;
            }
        }
    }
}
