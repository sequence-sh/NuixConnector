using System;
using System.Collections.Generic;
using Reductech.EDR.Processes;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    public class NuixErrorHandler : IErrorHandler
    {
        /// <summary>
        /// Create a new NuixErrorHandler.
        /// </summary>
        private NuixErrorHandler() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static IErrorHandler Instance = new NuixErrorHandler();

        public bool ShouldIgnoreError(string s)
        {
            if (NuixWarnings.Contains(s))
                return true;
            return false;
        }

        public static readonly ISet<string> NuixWarnings = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ERROR StatusLogger Log4j2 could not find a logging implementation. Please add log4j-core to the classpath. Using SimpleLogger to log to the console..."
            };
    }
}