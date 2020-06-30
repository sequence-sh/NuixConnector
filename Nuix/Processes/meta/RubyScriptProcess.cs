using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Immutable;
using Reductech.EDR.Processes.Mutable;
using Reductech.EDR.Processes.Mutable.Chain;

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

        /// <summary>
        /// The default required version of Nuix.
        /// 5.0 - this is required to check the nuix features.
        /// </summary>
        public static Version DefaultRequiredVersion { get; } = new Version(5,0);

        /// <summary>
        /// The required Nuix version for the process.
        /// </summary>
        internal abstract Version RequiredVersion { get; }

        private Version TrueRequiredVersion => RequiredVersion > DefaultRequiredVersion? RequiredVersion : DefaultRequiredVersion;

        /// <summary>
        /// The required Nuix features.
        /// </summary>
        internal abstract IReadOnlyCollection<NuixFeature> RequiredFeatures { get; }

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
        //TODO change to RubyMethodParameters

        internal virtual IEnumerable<string> GetAdditionalArgumentErrors()
        {
            yield break;
        }

        /// <inheritdoc />
        public override Result<IImmutableProcess<TOutput>> TryFreeze<TOutput>(IProcessSettings processSettings)
        {
            var errors = new List<string>();

            var parameterNames = new List<string> {"utilities" }; //always provide the utilities argument
            INuixProcessSettings nuixProcessSettings;

            if (!(processSettings is INuixProcessSettings nps))
            {
                nuixProcessSettings = new NuixProcessSettings(false, "", new Version(), new List<NuixFeature>() ); //dummy value for compiler
                errors.Add($"Process Settings must be an instance of {typeof(INuixProcessSettings).Name}");
            }
            else
            {
                nuixProcessSettings = nps;

                if (nuixProcessSettings.NuixVersion < TrueRequiredVersion)
                    errors.Add($"Your version of Nuix ({nuixProcessSettings.NuixVersion}) is less than the required version ({TrueRequiredVersion}) for the process: '{MethodName}'");

                var missingFeatures = RequiredFeatures.Except(nuixProcessSettings.NuixFeatures).Distinct().ToList();
                if(missingFeatures.Any())
                    errors.Add($"You lack the required features: '{string.Join(", ", missingFeatures.Select(x=>x.ToString()))}' for the process: '{MethodName}'");
            }

            var arguments = GetArgumentValues()
                .Select(x => new RubyMethodParameter(x.argumentName, x.argumentValue, x.valueCanBeNull))
                .ToList();


            foreach (var (parameterName, argumentValue, valueCanBeNull) in arguments)
            {
                if (string.IsNullOrWhiteSpace(argumentValue) && !valueCanBeNull)
                    errors.Add($"Argument '{parameterName}' must not be null"); //todo - this isn't the real argument names -> fix that
                parameterNames.Add(parameterName);
            }

            errors.AddRange(GetAdditionalArgumentErrors());

            if (errors.Any())
                return Result.Failure<IImmutableProcess<TOutput>>(string.Join("\r\n", errors));

            var methodBuilder = new StringBuilder();
            var methodHeader = $@"def {MethodName}({string.Join(",", parameterNames)})";

            methodBuilder.AppendLine(methodHeader);
            methodBuilder.AppendLine(ScriptText);
            methodBuilder.AppendLine("end");

            switch (ReturnType)
            {
                case NuixReturnType.Unit:
                {
                    var block = new BasicRubyBlock(MethodName, methodBuilder.ToString(), arguments, TrueRequiredVersion, RequiredFeatures);

                    var ip = new ImmutableRubyScriptProcess(new []{block}, nuixProcessSettings);

                    return TryConvertFreezeResult<TOutput, Unit>(ip);
                }
                case NuixReturnType.Boolean:
                {
                    var block = new BasicTypedRubyBlock<bool>(MethodName, methodBuilder.ToString(), arguments, TrueRequiredVersion, RequiredFeatures);
                    var ip = new ImmutableRubyScriptProcessTyped<bool>( block, nuixProcessSettings, TryParseBool);

                    return TryConvertFreezeResult<TOutput, bool>(ip);
                }
                case NuixReturnType.Integer:
                {
                    var block = new BasicTypedRubyBlock<int>(MethodName, methodBuilder.ToString(), arguments, TrueRequiredVersion, RequiredFeatures);
                    var ip = new ImmutableRubyScriptProcessTyped<int>( block, nuixProcessSettings, TryParseInt);

                    return TryConvertFreezeResult<TOutput, int>(ip);
                }
                case NuixReturnType.String:
                {
                    var block = new BasicTypedRubyBlock<string>(MethodName, methodBuilder.ToString(), arguments, TrueRequiredVersion, RequiredFeatures);
                    var ip = new ImmutableRubyScriptProcessTyped<string>( block, nuixProcessSettings, TryParseString);

                    return TryConvertFreezeResult<TOutput, string>(ip);
                }
                default:
                    return Result.Failure<IImmutableProcess<TOutput>>($"Cannot freeze a process with type {ReturnType}");
            }
        }

        /// <inheritdoc />
        public override Result<ChainLinkBuilder<TInput, TFinal>> TryCreateChainLinkBuilder<TInput, TFinal>()
        {
            return ReturnType switch
            {
                NuixReturnType.Unit =>
                new ChainLinkBuilder<TInput, Unit, TFinal, ImmutableRubyScriptProcess, RubyScriptProcess>(this),
                NuixReturnType.Boolean =>
                new ChainLinkBuilder<TInput, bool, TFinal, ImmutableRubyScriptProcessTyped<bool>, RubyScriptProcess>(this),
                NuixReturnType.Integer =>
                new ChainLinkBuilder<TInput, int, TFinal, ImmutableRubyScriptProcessTyped<int>, RubyScriptProcess>(this),
                NuixReturnType.String =>
                new ChainLinkBuilder<TInput, string, TFinal, ImmutableRubyScriptProcessTyped<string>, RubyScriptProcess>(this),
                _ => throw new ArgumentOutOfRangeException()
            };
        }


        internal static Result<string> TryParseString(string s)
        {
            return s == null ? Result.Failure<string>("String is null") : Result.Success(s);
        }

        internal static Result<int> TryParseInt(string s)
        {
            if (int.TryParse(s, out var i))
                return Result.Success(i);

            return Result.Failure<int>("Could not parse");
        }

        internal static Result<bool> TryParseBool(string s)
        {
            if (bool.TryParse(s, out var b))
                return Result.Success(b);

            return Result.Failure<bool>("Could not parse");
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetRequirements()
        {
            yield return $"Requires Nuix Version {TrueRequiredVersion}";

            foreach (var nuixFeature in RequiredFeatures.OrderBy(x=>x))
                yield return $"Requires Nuix Feature '{nuixFeature}'";
        }
    }
}