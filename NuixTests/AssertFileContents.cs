using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable;
using Reductech.EDR.Utilities.Processes.output;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

    internal class ImmutableAssertFileContents : ImmutableProcess<Unit> //TODO move into processes
    {
        /// <inheritdoc />
        public ImmutableAssertFileContents(string name, string filePath, string expectedContents) : base(name)
        {
            _filePath = filePath;
            _expectedContents = expectedContents;
        }

        private readonly string _filePath;

        private readonly string _expectedContents;

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<Unit>> Execute()
        {
            if (!File.Exists(_filePath))
                yield return ProcessOutput<Unit>.Error("File does not exist");
            else
            {
                var text = await File.ReadAllTextAsync(_filePath);

                if (text.Contains(_expectedContents))
                    yield return ProcessOutput<Unit>.Success(Unit.Instance);
                else
                {
                    yield return ProcessOutput<Unit>.Error("Contents do not match");
                }
            }
        }
    }

    internal class AssertFileContents : Process
    {
        
        [Required]
        [YamlMember]
#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
        public string FilePath { get; set; }

        /// <summary>
        /// The file must contain this string
        /// </summary>
        
        [Required]
        [YamlMember]
        public string ExpectedContents { get; set; }

        /// <inheritdoc />
        public override string GetReturnTypeInfo() => nameof(Unit);

        /// <inheritdoc />
        public override string GetName()
        {
            return "Assert file contains";
        }

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            var errors = new List<string>();

            if(string.IsNullOrWhiteSpace(FilePath)) errors.Add("FilePath is empty");
            if(string.IsNullOrWhiteSpace(ExpectedContents)) errors.Add("ExpectedContents is empty");

            if (errors.Any())
                return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList(errors));

            return Result.Success<ImmutableProcess, ErrorList>(new ImmutableAssertFileContents(GetName(), FilePath, ExpectedContents));
        }
    }
}
