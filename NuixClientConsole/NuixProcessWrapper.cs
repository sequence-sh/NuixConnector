using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Namotion.Reflection;
using NuixClient.Processes;
using YamlDotNet.Serialization;

namespace NuixClientConsole
{
    public class NuixProcessWrapper : IRunnable
    {
        private readonly Type _processType;

        public NuixProcessWrapper(Type processType)
        {
            _processType = processType;

            RelevantProperties = _processType.GetProperties()
                .Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(YamlMemberAttribute)))
                .Where(x => x.Name != nameof(Orchestration.Processes.Process.Conditions));
        }

        public string Name => _processType.Name;
        public string Summary => _processType.GetXmlDocsSummary();

        public Result<Func<object?>, List<string>> TryGetInvocation(IReadOnlyDictionary<string, string> dictionary)
        {
            var errors = new List<string>();
            var usedArguments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var instance = Activator.CreateInstance(_processType) as RubyScriptProcess;
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
                        errors.Add($"Could not parse '{v}' as {property.PropertyType.Name}");
                }
                else if (property.CustomAttributes.Any(att=>att.AttributeType == typeof(RequiredAttribute)))
                    errors.Add($"Argument '{property.Name}' of type {property.PropertyType.Name} is required");
            }

            var extraArguments = dictionary.Keys.Where(k => !usedArguments.Contains(k)).ToList();
            errors.AddRange(extraArguments.Select(extraArgument => $"Could not understand argument '{extraArgument}'"));

            if (errors.Any())
                return Result.Failure<Func<object?>, List<string>>(errors);
            
            var func = new Func<object?>(()=> instance.Execute());

            return Result.Success<Func<object?>, List<string>>(func);
        }

        private IEnumerable<PropertyInfo> RelevantProperties { get; }

        public IEnumerable<IParameter> Parameters => RelevantProperties.Select(x => new PropertyWrapper(x)).ToList();

        private class PropertyWrapper : IParameter
        {
            private readonly PropertyInfo _propertyInfo;

            public PropertyWrapper(PropertyInfo propertyInfo)
            {
                _propertyInfo = propertyInfo;
            }

            public string Name => _propertyInfo.Name;
            public string Summary => _propertyInfo.GetXmlDocsSummary();
            public Type Type => _propertyInfo.GetType();
        }
    }
}