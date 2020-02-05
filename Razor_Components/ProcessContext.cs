using System;
using System.Collections;
using System.Collections.Generic;

namespace Razor_Components
{
    /// <summary>
    /// Contains information about all the processes that have been created
    /// </summary>
    public sealed class ProcessContext : IDisposable, IEnumerable<Process>
    {
        private readonly ICollection<Process> _processes = new List<Process>();
        
        /// <summary>
        /// Adds another process
        /// </summary>
        /// <param name="process"></param>
        public void AddProcess(Process process)
        {
            _processes.Add(process);
        }

        /// <summary>
        /// Gets rid of this process context
        /// </summary>
        public void Dispose()
        {
            foreach (var process in _processes)
            {
                process.Dispose();
            }

            _processes.Clear();
        }

        IEnumerator<Process> IEnumerable<Process>.GetEnumerator()
        {
            return _processes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _processes.GetEnumerator();
        }
    }
}