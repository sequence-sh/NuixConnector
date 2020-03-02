using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using CSharpFunctionalExtensions;
using Namotion.Reflection;

namespace NuixClientConsole
{
    public class MethodWrapper : IRunnable
    {
        private readonly MethodInfo _methodInfo;

        public MethodWrapper(MethodInfo methodInfo)
        {
            _methodInfo = methodInfo;
        }

        public string Name => _methodInfo.Name;
        public string Summary => _methodInfo.GetXmlDocsSummary();

        public Result<Func<object?>, List<string?[]>> TryGetInvocation(IReadOnlyDictionary<string, string> dictionary)
        {
            var errors = new List<string?[]>();
            var usedArguments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            var argumentsToUse = new List<object?>();

            foreach (var parameter in _methodInfo.GetParameters())
            {
                if (parameter.Name == null) continue; //skip unnamed arguments?

                if (dictionary.TryGetValue(parameter.Name, out var v))
                {
                    usedArguments.Add(parameter.Name);
                    var (parsed,_, vObject ) = ArgumentHelpers.TryParseArgument(v, parameter.ParameterType);
                    if (parsed)
                        argumentsToUse.Add(vObject);
                    else
                        errors.Add(new []{parameter.Name, parameter.ParameterType.Name, $"Could not parse '{v}'" });
                }
                else if (parameter.HasDefaultValue)
                    argumentsToUse.Add(parameter.DefaultValue);
                else
                    errors.Add(new []{parameter.Name, parameter.ParameterType.Name, "Is required"});
            }
            
            

            var extraArguments = dictionary.Keys.Where(k => !usedArguments.Contains(k)).ToList();
            errors.AddRange(extraArguments.Select(extraArgument => new[] {extraArgument, null, "Not a valid argument"}));

            if (errors.Any())
                return Result.Failure<Func<object?>, List<string?[]>>(errors);
            
            var func = new Func<object?>(()=>_methodInfo.Invoke(null, argumentsToUse.ToArray()));

            return Result.Success<Func<object?>, List<string?[]>>(func);
        }

        public IEnumerable<IParameter> Parameters => _methodInfo.GetParameters().Select(x=> new ParameterWrapper(x));

        private class ParameterWrapper : IParameter
        {
            private readonly ParameterInfo _parameterInfo;

            public ParameterWrapper(ParameterInfo parameterInfo)
            {
                _parameterInfo = parameterInfo;
            }
            
            public string Name
            {
                get
                {
                    Debug.Assert(_parameterInfo.Name != null, "_parameterInfo.Name != null");
                    return _parameterInfo.Name;
                }
            }

            public string Summary => _parameterInfo.GetXmlDocs();
            public Type Type => _parameterInfo.ParameterType;
        }
    }
}