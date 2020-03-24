using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// A process that runs a ruby script against NUIX
    /// </summary>
    public abstract class RubyScriptProcess : Process
    {
        /// <summary>
        /// Checks if the current set of arguments is valid
        /// </summary>
        /// <returns></returns>
        internal abstract string ScriptName { get; }

        /// <summary>
        /// Get arguments that will be given to the nuix script.
        /// </summary>
        /// <returns></returns>
        internal abstract IEnumerable<(string arg, string val)> GetArgumentValuePairs();

        //Some processes will override this
        internal virtual Result<ImmutableProcess, ErrorList> TryGetImmutableProcess(string name, string nuixExeConsolePath,
            IReadOnlyCollection<string> arguments)
        {
            return Result.Success<ImmutableProcess, ErrorList>(new ImmutableRubyScriptProcess(name, nuixExeConsolePath, arguments));
        }

        internal virtual IEnumerable<string> GetAdditionalArgumentErrors()
        {
            yield break;
        }

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            var argsResult = TryGetArguments(processSettings, ScriptName, GetArgumentValuePairs(), GetAdditionalArgumentErrors());

            if (argsResult.IsFailure)
                return argsResult.ConvertFailure<ImmutableProcess>();

            var ip = TryGetImmutableProcess(GetName(), argsResult.Value.nuixExePath, argsResult.Value.arguments);

            return ip;
        }


        private static Result<(string nuixExePath, IReadOnlyCollection<string> arguments) , ErrorList> TryGetArguments(
            IProcessSettings processSettings,
            string scriptName,
            IEnumerable<(string arg, string val)> parameters,
            IEnumerable<string> additionalErrors)
        {
            var errors = new List<string>();

            var arguments = new List<string>();
            var nuixExePath = "";

            if (!(processSettings is INuixProcessSettings nps))
                errors.Add($"Process Settings must be an instance of {typeof(INuixProcessSettings).Name}");
            else
            {
                if (string.IsNullOrWhiteSpace(nps.NuixExeConsolePath))
                    errors.Add($"'{nameof(nps.NuixExeConsolePath)}' must not be empty");
                else
                    nuixExePath = nps.NuixExeConsolePath;

                if (nps.UseDongle)
                {
                    // ReSharper disable once StringLiteralTypo
                    arguments.Add("-licencesourcetype");
                    arguments.Add("dongle");  
                }
            }

            var currentDirectory = System.AppContext.BaseDirectory;
            var scriptPath = Path.Combine(currentDirectory,  "scripts", scriptName);

            if (!string.IsNullOrWhiteSpace(scriptPath))
                arguments.Add(scriptPath);

            foreach (var (key, value) in parameters)
            {
                arguments.Add(key);
                if (string.IsNullOrWhiteSpace(value))
                {
                    errors.Add($"Argument '{key}' must not be null"); //todo - this may not actually be very helpful to users with the current argument names
                }
                else
                    arguments.Add(value.Replace(@"\", @"\\")); //Escape backslashes
            }

            errors.AddRange(additionalErrors);

            if (errors.Any())
                return Result.Failure<(string nuixExePath, IReadOnlyCollection<string> arguments), ErrorList>(
                    new ErrorList(errors));
            else
                return Result.Success<(string nuixExePath, IReadOnlyCollection<string> arguments), ErrorList>((
                    nuixExePath, arguments));
        }
    }
}