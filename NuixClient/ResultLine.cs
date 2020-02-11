using System;
using System.Collections.Generic;
using System.Text;

namespace NuixClient
{
    /// <summary>
    /// A line from the result of an operation.
    /// </summary>
    public class ResultLine
    {
        /// <summary>
        /// Create a new result line
        /// </summary>
        /// <param name="isSuccess"></param>
        /// <param name="line"></param>
        public ResultLine(bool isSuccess, string line)
        {
            IsSuccess = isSuccess;
            Line = line;
        }

        /// <summary>
        /// A line in the result
        /// </summary>
        public string Line { get; set; }

        /// <summary>
        /// Whether this is part of a successful result. If this is false, it probably means an error
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// A string representation of this result line
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Line;
        }
    }
}
