using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.mutable;
using Process = Reductech.EDR.Utilities.Processes.mutable.Process;
using Reductech.Utilities.InstantConsole;

namespace Reductech.EDR.Connectors.Nuix.Console
{
    public sealed class ProcessWrapper<T> : YamlObjectWrapper, IRunnable where T : IProcessSettings
    {
        private readonly Type _processType;
        private readonly T _processSettings;

        public ProcessWrapper(Type processType, T processSettings, DocumentationCategory category)
        : base(processType, category)
        {
            _processType = processType;
            _processSettings = processSettings;
        }

        public Result<Func<object?>, List<string?[]>> TryGetInvocation(IReadOnlyDictionary<string, string> dictionary)
        {
            var errors = new List<string?[]>();
            var usedArguments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var instance = Activator.CreateInstance(_processType) as Process;
            Debug.Assert(instance != null, nameof(instance) + " != null");

            foreach (var property in RelevantProperties)
            {
                if (dictionary.TryGetValue(property.Name, out var v))
                {
                    usedArguments.Add(property.Name);
                    var (parsed, _, vObject ) = ArgumentHelpers.TryParseArgument(v, property.PropertyType);
                    if (parsed)
                        property.SetValue(instance, vObject);
                    else
                        errors.Add(new []{property.Name, property.PropertyType.Name, $"Could not parse '{v}'" });
                }
                else if (property.CustomAttributes.Any(att=>att.AttributeType == typeof(RequiredAttribute)))
                    errors.Add(new []{property.Name, property.PropertyType.Name, "Is required"});
            }

            var extraArguments = dictionary.Keys.Where(k => !usedArguments.Contains(k)).ToList();
            errors.AddRange(extraArguments.Select(extraArgument => new[] {extraArgument, null, "Not a valid argument"}));

            if (errors.Any())
                return Result.Failure<Func<object?>, List<string?[]>>(errors);


            var (isSuccess, _, value, error) = instance.TryFreeze<Unit>(_processSettings);

            if (isSuccess)
            {
                var func = new Func<object?>(() => value.Execute());

                return Result.Success<Func<object?>, List<string?[]>>(func);
            }

            return Result.Failure<Func<object?>, List<string?[]>>(new List<string?[]> {new []{error}});
        }

    }
}