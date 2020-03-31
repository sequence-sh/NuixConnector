using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// A process that runs a ruby script against NUIX
    /// </summary>
    public abstract class RubyScriptProcess : Process
    {
        /// <summary>
        /// Checks if the current set of arguments is valid.
        /// </summary>
        /// <returns></returns>
        internal abstract string ScriptText { get; }

        internal abstract string MethodName { get; }

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);
        
        /// <summary>
        /// The type that this method returns within Nuix.
        /// </summary>
        protected abstract NuixReturnType ReturnType { get; }

        /// <summary>
        /// Get arguments that will be given to the nuix script.
        /// </summary>
        /// <returns></returns>
        internal abstract IEnumerable<(string argumentName, string? argumentValue, bool valueCanBeNull)> GetArgumentValues();


        internal virtual IEnumerable<string> GetAdditionalArgumentErrors()
        {
            yield break;
        }

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            var errors = new List<string>();

            var parameterNames = new List<string> {"utilities" }; //always provide the utilities argument
            INuixProcessSettings nuixProcessSettings;

            if (!(processSettings is INuixProcessSettings nps))
            {
                nuixProcessSettings = new NuixProcessSettings(false, "");
                errors.Add($"Process Settings must be an instance of {typeof(INuixProcessSettings).Name}");
            }
            else
            {
                nuixProcessSettings = nps;
            }

            var arguments = GetArgumentValues();

            foreach (var (argumentName, argumentValue, valueCanBeNull) in GetArgumentValues())
            {
                if (string.IsNullOrWhiteSpace(argumentValue) && !valueCanBeNull) 
                    errors.Add($"Argument '{argumentName}' must not be null"); //todo - this isn't the real argument names -> fix that
                
                parameterNames.Add(argumentName);
            }

            errors.AddRange(GetAdditionalArgumentErrors());

            if (errors.Any())
                return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList(errors));

            var methodBuilder = new StringBuilder();
            var methodHeader = $@"def {MethodName}({string.Join(",", parameterNames)})";

            methodBuilder.AppendLine(methodHeader);
            methodBuilder.AppendLine(ScriptText);
            methodBuilder.AppendLine("end");

            switch (ReturnType)
            {
                case NuixReturnType.Unit:
                {
                    var methodCalls = new BasicMethodCall<Unit>(MethodName, methodBuilder.ToString(), arguments);

                    var ip = new ImmutableRubyScriptProcess(
                        nuixProcessSettings, new []{methodCalls});

                    return  Result.Success<ImmutableProcess, ErrorList>(ip);
                }
                case NuixReturnType.Boolean:
                {
                    var methodCall = new BasicMethodCall<bool>(MethodName, methodBuilder.ToString(), arguments);
                    var ip = new ImmutableRubyScriptProcessBool( methodCall, nuixProcessSettings);

                    return  Result.Success<ImmutableProcess, ErrorList>(ip);
                }
                case NuixReturnType.Integer:
                {
                    var methodCall = new BasicMethodCall<int>(MethodName, methodBuilder.ToString(), arguments);
                    var ip = new ImmutableRubyScriptProcessInt( methodCall, nuixProcessSettings);

                    return  Result.Success<ImmutableProcess, ErrorList>(ip);
                }
                case NuixReturnType.String:
                {
                    var methodCall = new BasicMethodCall<string>(MethodName, methodBuilder.ToString(), arguments);
                    var ip = new ImmutableRubyScriptProcessString( methodCall, nuixProcessSettings);

                    return  Result.Success<ImmutableProcess, ErrorList>(ip);
                }
                default:
                    return Result.Failure<ImmutableProcess, ErrorList>(
                        new ErrorList($"Cannot freeze a process with type {ReturnType}"));
            }

            
        }
    }
}