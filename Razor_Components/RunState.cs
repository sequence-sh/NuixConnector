namespace Razor_Components
{
    /// <summary>
    /// The state of a process
    /// </summary>
    public enum RunState
    {
        /// <summary>
        /// The process is ready to run
        /// </summary>
        Ready,
        /// <summary>
        /// The process is running
        /// </summary>
        Running,
        /// <summary>
        /// The process has finished
        /// </summary>
        Finished,
        /// <summary>
        /// The process has been cancelled
        /// </summary>
        Cancelled,
        /// <summary>
        /// Something went wrong
        /// </summary>
        Error
    }
}