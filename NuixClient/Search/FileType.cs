using System.Text.RegularExpressions;
using Orchestration;

namespace NuixClient.Search
{
    /// <summary>
    /// The type of the file
    /// </summary>
    public class FileType
    {
        /// <summary>
        /// Create a new FileType
        /// </summary>
        public FileType(string category, string type)
        {
            Category = category;
            Type = type;
        }

        /// <summary>
        /// The category of the file
        /// </summary>
        public readonly string Category;

        /// <summary>
        /// The type of the file
        /// </summary>
        public readonly string Type;

        /// <summary>
        /// Render this FileType as a string
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Category + "/" + Type;
        }
        
        /// <summary>
        /// Create a range from a string
        /// </summary>
        /// <returns></returns>
        public static Result<FileType> TryParse(string str)
        {
            var match = FileTypeRegex.Match(str);

            return match.Success
                ? Success<FileType>.Success(new FileType(match.Groups["Category"].Value,
                    match.Groups["Type"].Value))
                : Result<FileType>.Failure($"Could not parse '{str}' as a file type.");
        }

        private static readonly Regex FileTypeRegex = new Regex(@"\A(?<Category>[\w-_]+)\/(?<Type>[\w-_\.]+)\Z", RegexOptions.IgnoreCase | RegexOptions.Compiled);
    }
}