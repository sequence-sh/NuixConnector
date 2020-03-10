using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Namotion.Reflection;
using Reductech.EDR.Connectors.Nuix.processes;
using Reductech.Utilities.InstantConsole;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.Console
{
    public class NuixProcessWrapper : IRunnable
    {
        private readonly Type _processType;
        private readonly INuixProcessSettings _nuixProcessSettings;

        public NuixProcessWrapper(Type processType, INuixProcessSettings nuixProcessSettings)
        {
            _processType = processType;
            _nuixProcessSettings = nuixProcessSettings;

            RelevantProperties = _processType.GetProperties()
                .Where(x => x.CustomAttributes.Any(y => y.AttributeType == typeof(YamlMemberAttribute)));


            var instance = Activator.CreateInstance(processType);
            Parameters = RelevantProperties.Select(propertyInfo => 
                new PropertyWrapper(propertyInfo, propertyInfo.GetValue(instance)?.ToString()  )).ToList();
        }

        public string Name => _processType.Name;
        public string Summary => _processType.GetXmlDocsSummary();

        public Result<Func<object?>, List<string?[]>> TryGetInvocation(IReadOnlyDictionary<string, string> dictionary)
        {
            var errors = new List<string?[]>();
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
                        errors.Add(new []{property.Name, property.PropertyType.Name, $"Could not parse '{v}'" });
                }
                else if (property.CustomAttributes.Any(att=>att.AttributeType == typeof(RequiredAttribute)))
                    errors.Add(new []{property.Name, property.PropertyType.Name, "Is required"});
            }

            var extraArguments = dictionary.Keys.Where(k => !usedArguments.Contains(k)).ToList();
            errors.AddRange(extraArguments.Select(extraArgument => new[] {extraArgument, null, "Not a valid argument"}));

            if (errors.Any())
                return Result.Failure<Func<object?>, List<string?[]>>(errors);
            
            var func = new Func<object?>(()=> instance.Execute(_nuixProcessSettings));

            return Result.Success<Func<object?>, List<string?[]>>(func);
        }

        private IEnumerable<PropertyInfo> RelevantProperties { get; }

        public IEnumerable<IParameter> Parameters { get; }
            
            

        private class PropertyWrapper : IParameter
        {
            private readonly PropertyInfo _propertyInfo;

            public PropertyWrapper(PropertyInfo propertyInfo, string? defaultValueString)
            {
                _propertyInfo = propertyInfo;
                DefaultValueString = defaultValueString;
            }

            public string Name => _propertyInfo.Name;
            public string Summary => _propertyInfo.GetXmlDocsSummary();
            public Type Type => _propertyInfo.PropertyType;
            public bool Required => _propertyInfo.CustomAttributes.Any(att=>att.AttributeType == typeof(RequiredAttribute));
            public string? DefaultValueString { get; }
        }
    }
}