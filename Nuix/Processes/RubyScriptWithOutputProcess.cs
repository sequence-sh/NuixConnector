using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.immutable;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.processes
{

    /// <summary>
    /// A process that runs a ruby script in Nuix and also writes something to a file on the file system.
    /// </summary>
    public abstract class RubyScriptWithOutputProcess : RubyScriptProcess
    {
        /// <inheritdoc />
        internal override Result<ImmutableProcess, ErrorList> TryGetImmutableProcess(string name, string nuixExeConsolePath, IReadOnlyCollection<string> arguments)
        {
            if (string.IsNullOrWhiteSpace(OutputFolder))
                return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList($"{nameof(OutputFolder)} field must not be empty."));

            if (!Directory.Exists(OutputFolder))
                return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList($"{OutputFolder} does not exist."));


            return Result.Success<ImmutableProcess, ErrorList>(
                new ImmutableRubyScriptWithOutputProcess(name, nuixExeConsolePath, arguments, OutputFolder));
        }



        /// <summary>
        /// The path to the folder to put the output files in.
        /// </summary>
        [Required]
        
        [YamlMember(Order = 4)]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string OutputFolder { get; set; }
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
    }
}