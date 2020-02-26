using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Namotion.Reflection;

namespace NuixClientConsole
{
    public static class ConsoleView
    {
        private static async IAsyncEnumerable<string> HandleMethod(MethodBase method,
            IReadOnlyDictionary<string, string> dictionary)
        {
            var allGood = true;
            var usedArguments = new HashSet<string>();

            var argumentsToUse = new List<object?>();

            foreach (var parameter in method.GetParameters())
            {
                if (parameter.Name == null) continue; //skip unnamed arguments?

                if (dictionary.TryGetValue(parameter.Name, out var v))
                {
                    usedArguments.Add(parameter.Name);
                    if (TryParseArgument(v, parameter.ParameterType, out var vObject))
                    {
                        argumentsToUse.Add(vObject);
                    }
                    else
                    {
                        allGood = false;
                        yield return $"Could not parse '{v}' as {parameter.ParameterType.Name}";
                    }
                }
                else if (parameter.HasDefaultValue)
                {
                    argumentsToUse.Add(parameter.DefaultValue);
                }
                else
                {
                    allGood = false;
                    yield return $"Argument '{parameter.Name}' of type {parameter.ParameterType.Name} is required";
                }
            }

            var extraArguments = dictionary.Keys.Where(k => !usedArguments.Contains(k)).ToList();
            foreach (var extraArgument in extraArguments)
            {
                allGood = false;
                yield return $"Could not understand argument '{extraArgument}'";
            }

            if (!allGood) yield break;
            object firstResult;

            var startTime = DateTime.Now;

            try
            {
#pragma warning disable CS8600 //We assume the method is static
                firstResult = method.Invoke(null, argumentsToUse.ToArray());
#pragma warning restore CS8600
            }
#pragma warning disable CA1031 // Do not catch general exception types - I don't know what kind of exception could be thrown by my method
            catch (Exception e)
            {
                firstResult = e.Message;
            }

            //This block is to handle asynchronous methods with return tasks
            switch (firstResult)
            {
                //The previous method didn't necessarily do any work - do the work here
                case Task task:
                {
                    await task;
                    var resultProperty = task.GetType().GetProperty("Result");

                    var rString = resultProperty?.GetValue(task)?.ToString();
                    if(rString != null) yield return rString;
                    break;
                }
                case string s:
                    yield return s;
                    break;
                case ICollection collection:
                    var resultStrings = collection.Cast<object>().Select(x => x.ToString() ?? string.Empty);
                    foreach (var resultString in resultStrings)
                        yield return resultString;
                    break;
                case IAsyncEnumerable<object> asyncEnumerable:
                {
                    await foreach (var obj in asyncEnumerable)
                        yield return obj.ToString() ?? string.Empty;
                    break;
                }
                case IEnumerable enumerable: //no need to handle this in a different thread
                {
                    foreach (var obj in enumerable)
                        yield return obj?.ToString() ?? string.Empty;

                    break;
                }
                default:
                    yield return firstResult?.ToString() ?? string.Empty;
                    break;
            }

            var now = DateTime.Now;
            yield return $"Finished in {now.Subtract(startTime).TotalSeconds} seconds";
        }
        

        /// <summary>
        /// Makes dictionary of arguments or returns null if there is a problem.
        /// Ignores the first argument (assumed to be the method name)
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        private static IReadOnlyDictionary<string, string>? MakeArgumentDictionary(IReadOnlyList<string> args)
        {
            if (args.Count % 2 == 0)
                return null; //should always be an odd number of arguments

            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 1; i < args.Count - 1; i+=2)
            {
                var paramName = args[i];
                var paramValue = args[i + 1];

                if (!paramName.StartsWith('-') || paramValue.StartsWith('-'))
                    return null; //parameters must start with a dash. non-parameters must not

                var realParamName = paramName.Substring(1);

                result.Add(realParamName, paramValue);
            }

            return result;
        }

        /// <summary>
        /// Runs the method. Returns output lines asynchronously
        /// </summary>
        /// <param name="args"></param>
        /// <param name="methods"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<string> Run(string[] args, IEnumerable<MethodInfo> methods)
        {
            //the fist argument should be the method name. The remaining arguments should be '-parameterName parameterValue'

            if (args.Length == 0)
            {
                yield return "Please provide the name of the method as an argument or ask for 'help' for a list of methods.";
            }
            else
            {
                var methodName = args.First();

                if (args.Length == 1 && methodName.Equals("help", StringComparison.OrdinalIgnoreCase))
                {
                    yield return "Possible methods are:";
                    foreach (var method in methods)
                    {
                        var summary = method.GetXmlDocsSummary();
                        if(string.IsNullOrWhiteSpace(summary))
                            yield return method.Name;
                        else
                            yield return $"{method.Name} - {summary}";
                    }
                }
                else
                {
                    var possibleMethods = methods.Where(x => x.Name == methodName).ToList();

                    if (possibleMethods.Count == 0)
                        yield return $"Could not find method with name '{methodName}'. Type 'help' for list of methods";
                    else if (possibleMethods.Count > 1)
                        yield return $"Too many methods with name '{methodName}'. Please fix the application";
                    else
                    {
                        var method = possibleMethods.Single();

                        if (args.Length == 2 && args[1].Equals("help", StringComparison.OrdinalIgnoreCase))
                        {
                            var summary = method.GetXmlDocsSummary();
                            if (string.IsNullOrWhiteSpace(summary))
                                yield return method.Name;
                            else
                                yield return $"{method.Name} - {summary}";

                            foreach (var parameter in method.GetParameters())
                            {
                                var parameterSummary = parameter.GetXmlDocs();
                                if (string.IsNullOrWhiteSpace(parameterSummary))
                                    yield return $"{parameter.Name} - {parameter.ParameterType.Name}";
                                else
                                    yield return
                                        $"{parameter.Name} - {parameter.ParameterType.Name} - {parameterSummary}";
                            }
                        }
                        else
                        {
                            var dictionary = MakeArgumentDictionary(args);
                            if (dictionary == null)
                                yield return "Please provide arguments in the form \"-argument value\"";
                            else
                            {
                                var handleMethodResult=  HandleMethod(method, dictionary);

                                await foreach (var s in handleMethodResult)
                                    yield return s;
                            }
                        }
                    }
                }
            }
        }


        private static bool TryParseArgument(string s, Type parameterType, out object? value)
        {
            var type = parameterType;

            var isNullable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (isNullable)
            {
                var underlyingType = Nullable.GetUnderlyingType(type);
                type = underlyingType ?? throw new ArgumentException("Problem converting isParameterNullable type.");
            }


            if (type.IsEnum)
            {
                return Enum.TryParse(parameterType, s, true, out value);
            }

            var (success, resultValue) = ParseArgument(type.Name, s);
            value = resultValue;
            return success;
        }

        private static (bool success, object? resultValue) ParseArgument(string underlyingTypeName, string s)
        {
            switch (underlyingTypeName)
            {
                case nameof(String): return (true, s);
                case nameof(Boolean): return (bool.TryParse(s, out var r1), r1);
                case nameof(Int16): return (short.TryParse(s, out var r2), r2);
                case nameof(Int32): return (int.TryParse(s, out var r3), r3);
                case nameof(Int64): return (long.TryParse(s, out var r4), r4);
                case nameof(Double): return (double.TryParse(s, out var r5), r5);
                case nameof(Decimal): return (decimal.TryParse(s, out var r6), r6);
                case nameof(Guid): return (Guid.TryParse(s, out var r7), r7);

                default:
                {
                    return (false, null);
                }
            }
        }


    }
}
