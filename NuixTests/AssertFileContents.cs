using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Runtime.Serialization;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    internal class AssertFileContents : Process
    {
        [DataMember]
        [Required]
        [YamlMember]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string FilePath { get; set; }

        /// <summary>
        /// The file must contain this string
        /// </summary>
        [DataMember]
        [Required]
        [YamlMember]
        public string ExpectedContents { get; set; }

#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        /// <inheritdoc />
        public override IEnumerable<string> GetArgumentErrors()
        {
            if (string.IsNullOrWhiteSpace(FilePath))
                yield return "FilePath is empty";
        }

        /// <inheritdoc />
        public override IEnumerable<string> GetSettingsErrors(IProcessSettings processSettings)
        {
            yield break;
        }

        /// <inheritdoc />
        public override string GetName()
        {
            return "Assert file contains";
        }

        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute(IProcessSettings processSettings)
        {
            if (!File.Exists(FilePath))
                yield return Result.Failure<string>("File does not exist");
            else
            {
                var text = await File.ReadAllTextAsync(FilePath);

                if (text.Contains(ExpectedContents))
                    yield return Result.Success("Contents Match");
                else
                {
                    yield return Result.Failure<string>("Contents do not match");
                }
            }

        }
    }
}
