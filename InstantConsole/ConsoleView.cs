using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;

namespace InstantConsole
{
    public static class ConsoleView
    {
        /// <summary>
        /// Uses the arguments to choose a method, set its parameters and run it, printing its output to the console.
        /// </summary>
        /// <param name="args"></param>
        /// <param name="methods"></param>
        public static void Run(string[] args, IEnumerable<IRunnable> methods)
        {
            var lines = ConsoleView.RunAsync(args, methods);

            var enumerator = lines.GetAsyncEnumerator();

            try
            {
                while (true)
                {
                    var nextTask = enumerator.MoveNextAsync().AsTask();
                    var next = nextTask.Result;
                    if (!next)
                        break;
                    Console.WriteLine(enumerator.Current);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }


        /// <summary>
        /// Runs the method. Returns output lines asynchronously
        /// </summary>
        /// <param name="args"></param>
        /// <param name="methods"></param>
        /// <returns></returns>
        private static async IAsyncEnumerable<string> RunAsync(IReadOnlyList<string> args, IEnumerable<IRunnable> methods)
        {
            //the fist argument should be the method name. The remaining arguments should be '-parameterName parameterValue'
            if (args.Count == 0 || args.Count == 1 && args.First().Equals("help", StringComparison.OrdinalIgnoreCase))
            {
                yield return "Possible methods are:";

                var ps = Prettifier.ArrangeIntoColumns(methods.Select(m => new[] {m.Name, m.Summary}));

                foreach (var prettyString in ps)
                    yield return prettyString;
            }
            else
            {
                var methodName = args.First();

                var possibleMethods = methods.Where(x => x.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase)).ToList();

                if (possibleMethods.Count == 0)
                    yield return $"Could not find method with name '{methodName}'. Type 'help' for list of methods";
                else if (possibleMethods.Count > 1)
                    yield return $"Too many methods with name '{methodName}'. Please fix the application";
                else
                {
                    var method = possibleMethods.Single();

                    if (args.Count == 2 && args[1].Equals("help", StringComparison.OrdinalIgnoreCase))
                    {
                        var rows = new List<string?[]>
                        {
                            new[] {method.Name, null, null, method.Summary},
                            Array.Empty<string>(), //empty line
                        };

                        rows.AddRange(method.Parameters.Select(p=> new []{p.Name, p.TypeName, p.Required? "Required": "",  p.Summary}));

                        var prettyStrings = Prettifier.ArrangeIntoColumns(rows);

                        foreach (var prettyString in prettyStrings)
                            yield return prettyString;
                    }
                    else
                    {
                        var dictionary = MakeArgumentDictionary(args);
                        if (dictionary == null)
                            yield return "Please provide arguments in the form \"-argument value\"";
                        else
                        {
                            var (_, isFailure, invocation, error) = method.TryGetInvocation(dictionary);

                            if (isFailure)
                            {
                                foreach (var s in Prettifier.ArrangeIntoColumns(error))
                                    yield return s;
                                yield break;
                            }

                            var results = Invoke(invocation);

                            await foreach (var (isSuccess, _, value, resultError) in results)
                                yield return isSuccess ? value : resultError;
                        }
                    }
                }
            }
        }


        private static async IAsyncEnumerable<Result<string>> Invoke(Func<object?> func)
        {
            object? firstResult;
            string? error;

            var stopWatch = Stopwatch.StartNew();

            try
            {
                firstResult = func.Invoke();
                error = null;
            }
#pragma warning disable CA1031 // Do not catch general exception types - I don't know what kind of exception could be thrown by my method
            catch (Exception e)
            {
                error = e.Message;
                firstResult = null;
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                yield return Result.Failure<string>(error);
                yield break;
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
                    if(rString != null) yield return Result.Success(rString);
                    break;
                }
                case string s:
                    yield return Result.Success(s);
                    break;
                case ICollection collection:
                    var resultStrings = collection.Cast<object>().Select(x => x.ToString() ?? string.Empty);
                    foreach (var resultString in resultStrings)
                        yield return Result.Success(resultString);
                    break;
                case IAsyncEnumerable<Result<string>> resultEnumerable:
                {
                    await foreach (var r in resultEnumerable)
                        yield return r;
                    break;
                }
                case IAsyncEnumerable<object> asyncEnumerable:
                {
                    await foreach (var obj in asyncEnumerable)
                        yield return Result.Success(obj.ToString()??string.Empty);
                    break;
                }
                case IEnumerable enumerable: //no need to handle this in a different thread
                {
                    foreach (var obj in enumerable)
                        yield return Result.Success(obj?.ToString() ?? string.Empty);

                    break;
                }
                default:
                    yield return Result.Success(firstResult?.ToString() ?? string.Empty);
                    yield return Result.Failure<string>($"Could not handle result of type '{firstResult?.GetType()}'");
                    break;
            }

            stopWatch.Stop();

            yield return Result.Success($"Finished in {stopWatch.Elapsed.TotalSeconds} seconds") ;
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
    }
}
