using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Razor_Components
{
    /// <summary>
    /// Represents the running of a method
    /// </summary>
    public sealed class Process : IDisposable
    {
        /// <summary>
        /// The method to run
        /// </summary>
        public MethodInfo Method { get; }
        /// <summary>
        /// The object to run the method on. Null if the method is static
        /// </summary>
        public object? ObjectInstance { get; }
        /// <summary>
        /// The parameters to give to the method
        /// </summary>
        public IReadOnlyDictionary<string, object?> Parameters { get; }

        /// <summary>
        /// Cancellation token for the method
        /// </summary>
        public CancellationTokenSource  CancellationTokenSource { get; }

        /// <summary>
        /// The state of this process
        /// </summary>
        public RunState RunState { get; private set; }

        /// <summary>
        /// The result of the process
        /// </summary>
        public List<string> ResultStrings { get; } = new List<string>();

        /// <summary>
        /// The date this process was created
        /// </summary>
        public DateTime DateCreated { get; }

        /// <summary>
        /// The date this was finished, cancelled, or threw an exception
        /// </summary>
        public DateTime? DateFinished { get; private set; }

        /// <summary>
        /// Creates a new process
        /// </summary>
        public Process(MethodInfo method, object? objectInstance,
            IReadOnlyDictionary<string, object?> parameters, CancellationTokenSource cancellationTokenSource)
        {
            DateCreated = DateTime.Now;
            Method = method;
            ObjectInstance = objectInstance;
            Parameters = parameters;
            CancellationTokenSource = cancellationTokenSource;
            RunState = RunState.Ready;
        }

        /// <summary>
        /// Runs the process
        /// </summary>
        public async void Run()
        {
            const double debounceInterval = 1000; //debounce for enumerables

            if (RunState == RunState.Ready)
            {
                RunState = RunState.Running;
                Update?.Invoke(UpdateEventArgs);

                try
                {
                    var firstResult = await Task.Run(() => Method.InvokeWithNamedParameters(
#pragma warning disable CS8604 // Possible null reference argument - this parameter can be null if the method is static
                        ObjectInstance,
#pragma warning restore CS8604 // Possible null reference argument.
                        Parameters), CancellationTokenSource.Token);
                    if (CancellationTokenSource.IsCancellationRequested)
                    {
                        RunState = RunState.Cancelled;
                        Update?.Invoke(UpdateEventArgs);
                    }
                    else
                    {
                        //This block is to handle asynchronous methods with return tasks
                        switch (firstResult)
                        {
                            //The previous method didn't necessarily do any work - do the work here
                            case Task task:
                            {
                                await task;
                                var resultProperty = task.GetType().GetProperty("Result");

                                ResultStrings.Add(resultProperty?.GetValue(task)?.ToString() ?? string.Empty);
                                break;
                            }
                            case string s:
                                ResultStrings.Add(s);
                                break;
                            case ICollection collection:
                                ResultStrings.AddRange(collection.Cast<object>()
                                    .Select(x => x.ToString() ?? string.Empty));
                                break;
                            case IAsyncEnumerable<object> asyncEnumerable:
                            {
                                var lastUpdate = DateTime.Now;

                                await foreach (var obj in asyncEnumerable)
                                {
                                    if (CancellationTokenSource.IsCancellationRequested) //probably not needed, but just in case
                                        break;

                                    ResultStrings.Add(obj.ToString() ?? string.Empty);
                                    if (DateTime.Now.Subtract(lastUpdate).TotalMilliseconds > debounceInterval)
                                        Update?.Invoke(UpdateEventArgs); //only update if some time has passed since last update

                                    lastUpdate = DateTime.Now;
                                }
                                break;
                            }
                            case IEnumerable enumerable:
                            {
                                var lastUpdate = DateTime.Now;

                                var asyncEnumerable =  EnumerableExtensions.AsAsyncEnumerable(enumerable, CancellationTokenSource.Token);

                                await foreach (var obj in asyncEnumerable)
                                {
                                    if (CancellationTokenSource.IsCancellationRequested)
                                        break;

                                    ResultStrings.Add(obj?.ToString() ?? string.Empty);
                                    if (DateTime.Now.Subtract(lastUpdate).TotalMilliseconds > debounceInterval) 
                                        Update?.Invoke(UpdateEventArgs); //only update if some time has passed since last update
                                }
                                break;
                            }
                            default:
                                ResultStrings.Add(firstResult?.ToString() ?? string.Empty);
                                break;
                        }

                        RunState = CancellationTokenSource.IsCancellationRequested ? RunState.Cancelled : RunState.Finished;

                        DateFinished = DateTime.Now;
                        Update?.Invoke(UpdateEventArgs);
                    }

                }
                catch (TaskCanceledException tce)
                {
                    RunState = RunState.Cancelled;
                    ResultStrings.Add(tce.Message);
                    DateFinished = DateTime.Now;
                    Update?.Invoke(UpdateEventArgs);
                    
                }
#pragma warning disable CA1031 // Do not catch general exception types - I don't know what kind of exception could be thrown by my method
                catch (Exception e)
                {
                    RunState = RunState.Error;
                    ResultStrings.Add(e.Message);
                    DateFinished = DateTime.Now;
                    Update?.Invoke(UpdateEventArgs);
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
            else
            {
                throw new Exception("Process is not ready"); //This should never happen
            }
        }

        /// <summary>
        /// Event to subscribe to
        /// </summary>
        public event UpdateHandler? Update;

        /// <summary>
        /// Used for every update
        /// </summary>
        private static readonly EventArgs UpdateEventArgs = new EventArgs();

        /// <summary>
        /// Event to listen for
        /// </summary>
        /// <param name="e"></param>
        public delegate void UpdateHandler(EventArgs e);

        /// <summary>
        /// Disposes of this process
        /// </summary>
        public void Dispose()
        {
            CancellationTokenSource.Cancel();
            RunState = RunState.Cancelled;
        }
    }
}
