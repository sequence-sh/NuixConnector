using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Orchestration
{

    /// <summary>
    /// Anything that implements ReadLineAsync
    /// </summary>
    internal interface IStreamReader
    {
        /// <summary>
        /// Reads a line of characters asynchronously and returns the data as a string
        /// </summary>
        /// <returns></returns>
        Task<string?> ReadLineAsync();
    }

    internal class WrappedStreamReader : IStreamReader
    {
        private readonly StreamReader _underlying;

        public WrappedStreamReader(StreamReader underlying)
        {
            _underlying = underlying;
        }

        public async Task<string?> ReadLineAsync()
        {
            return await _underlying.ReadLineAsync();
        }
    }

    /// <summary>
    /// Reads lines from several StreamReaders in the order that they arrive
    /// </summary>
    internal class MultiStreamReader : IStreamReader
    {
        private readonly List<(IStreamReader streamReader, Task<string?>? task)> _streamReaderTaskPairs;

        /// <summary>
        /// Create a new MultiStreamReader
        /// </summary>
        /// <param name="streamReaders"></param>
        public MultiStreamReader(IEnumerable<IStreamReader> streamReaders)
        {
            _streamReaderTaskPairs = streamReaders.Select(x => (x, null as Task<string?>)).ToList();
        }


        /// <summary>
        /// Read the next line from any of these stream readers. Returns null if all of them are finished
        /// </summary>
        /// <returns></returns>
        public async Task<string?> ReadLineAsync()
        {
            var awaitingTasks = new List<Task<string?>>();

            for (var i = 0; i < _streamReaderTaskPairs.Count; i++) //go through all stream readers
            {
                var (streamReader, task1) = _streamReaderTaskPairs[i];
                if (task1 == null)
                {
                    //this stream reader has no yet been asked for the next line
                    var task = streamReader.ReadLineAsync();
                    //task.Start();

                    _streamReaderTaskPairs[i] = (streamReader, task);

                    awaitingTasks.Add(task);
                }
                else
                {
                    //this stream reader has been asked for the next line during a previous call to this function
                    awaitingTasks.Add(task1);
                }
            }

            while (awaitingTasks.Any()) //loop until a string is returned that is not null
            {
                var firstCompletedTask = await Task.WhenAny(awaitingTasks);
                var firstResult = firstCompletedTask.Result;


                for (var i = 0; i < _streamReaderTaskPairs.Count; i++)
                {
                    var (streamReader, task) = _streamReaderTaskPairs[i];
                    if (task != firstCompletedTask) continue;
                    if (firstResult == null) _streamReaderTaskPairs.RemoveAt(i); //stream is finished - remove it
                    else _streamReaderTaskPairs[i] = (streamReader, null); //we've dealt with this task - remove it
                    break;
                }

                if (firstResult != null) return firstResult; //return this result

                awaitingTasks.Remove(firstCompletedTask); //remove this task
            }

            return null; //all readers are finished
        }
    }
}
