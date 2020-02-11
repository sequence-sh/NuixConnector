using System.Text.RegularExpressions;
using JetBrains.Annotations;

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
        /// <param name="category"></param>
        /// <param name="type"></param>
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
        [ContractAnnotation("=>true,range:notNull; =>false,range:null;")]
        public static bool TryParse(string str, out FileType? range)
        {
            var match = FileTypeRegex.Match(str);

            if (match.Success)
            {
                range = new FileType(match.Groups["Category"].Value, match.Groups["Type"].Value);
                return true;
            }
            else
            {
                range = null;
                return false;
            }
        }

        private static readonly Regex FileTypeRegex = new Regex(@"\A(?<Category>\w+)\/(?<Type>\w+)\Z", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }
}