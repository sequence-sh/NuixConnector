namespace NuixClient.Orchestration
{
    /// <summary>
    /// A condition that requires that a particular file exists
    /// </summary>
    public class FileExistsCondition : ICondition
    {
        /// <summary>
        /// Create a new file-exists condition
        /// </summary>
        /// <param name="filePath"></param>
        public FileExistsCondition(string filePath)
        {
            FilePath = filePath;
        }

        /// <summary>
        /// The path of the file to check
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// 
        /// </summary>
        public string Description => $"{FilePath} exists";

        /// <summary>
        /// Does the file exist
        /// </summary>
        /// <returns></returns>
        public bool IsMet()
        {
            return System.IO.File.Exists(FilePath);
        }
    }
}