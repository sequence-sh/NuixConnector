using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal.Errors;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    internal static class RubyScriptCompilationHelper
    {
        /// <summary>
        /// The name of the parameters hash set
        /// </summary>
        public const string HashSetName = "params";

        public static string CompileScriptSetup(IRubyBlock rubyBlock)
        {
            var scriptStringBuilder = new StringBuilder();

            scriptStringBuilder.AppendLine($"#{rubyBlock.Name}");
            scriptStringBuilder.AppendLine();

            var highestVersion =
                rubyBlock.FunctionDefinitions
                    .Select(x => x.RequiredNuixVersion)
                    .OrderByDescending(x => x).FirstOrDefault();

            if (highestVersion != null)
            {
                scriptStringBuilder.AppendLine($"requiredNuixVersion = '{highestVersion}'");
                scriptStringBuilder.AppendLine("if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)");
                scriptStringBuilder.AppendLine("\traise \"Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required\"");
                scriptStringBuilder.AppendLine("\texit");
                scriptStringBuilder.AppendLine("end");
                scriptStringBuilder.AppendLine();
            }

            var requiredFeatures = rubyBlock.FunctionDefinitions
                .SelectMany(x => x.RequiredNuixFeatures)
                .Distinct().OrderBy(x=>x).ToList();

            if (requiredFeatures.Any())
            {
                var featuresArray = string.Join(", ", requiredFeatures.Select(rf => $"'{rf}'"));
                scriptStringBuilder.AppendLine($"requiredFeatures = Array[{featuresArray}]");
                scriptStringBuilder.AppendLine("requiredFeatures.each do |feature|");
                scriptStringBuilder.AppendLine("\tif !utilities.getLicence().hasFeature(feature)");
                scriptStringBuilder.AppendLine("\t\tputs \"Nuix Feature #{feature} is required but not available.\"");
                scriptStringBuilder.AppendLine("\t\texit");
                scriptStringBuilder.AppendLine("\tend");
                scriptStringBuilder.AppendLine("end");
                scriptStringBuilder.AppendLine();
            }

            scriptStringBuilder.AppendLine("require 'optparse'");

            //Parse options
            scriptStringBuilder.AppendLine(HashSetName + " = {}");
            scriptStringBuilder.AppendLine("OptionParser.new do |opts|");


            var suffixer = new Suffixer();
            rubyBlock.WriteOptParseLines(HashSetName, new IndentationStringBuilder(scriptStringBuilder, 1), suffixer );

            scriptStringBuilder.AppendLine("end.parse!");
            scriptStringBuilder.AppendLine();


            return scriptStringBuilder.ToString();
        }

        public const string UtilitiesParameterName = "utilities";

        /// <summary>
        /// Compiles the text for all the called script functions.
        /// </summary>
        public static string CompileScriptFunctionText(IRubyBlock rubyBlock)
        {
            var stringBuilder = new StringBuilder();

            foreach (var function in rubyBlock.FunctionDefinitions.Distinct())
            {
                var parameters = function.Arguments.Select(x => x.ParameterName);


                if (function.RequireUtilities)
                    parameters = parameters.Prepend(UtilitiesParameterName);


                var methodHeader = $@"def {function.FunctionName}({string.Join(",", parameters)})";

                stringBuilder.AppendLine(methodHeader);
                stringBuilder.AppendLine(IndentFunctionText(function.FunctionText));
                stringBuilder.AppendLine("end");
                stringBuilder.AppendLine();
            }

            var s = stringBuilder.ToString();

            return s;


            static string IndentFunctionText(string functionText)
            {
                functionText = functionText.Trim('\n', '\r');

                if (functionText.StartsWith('\t') || functionText.StartsWith(' '))
                    return functionText;

                var indentedText = string.Join(Environment.NewLine,
                    functionText.Split(Environment.NewLine).Select(x => '\t' + x));

                return indentedText;
            }
        }

        public static async Task<Result<IReadOnlyCollection<string>, IErrorBuilder>> PrepareScriptAsync(IRubyBlock block, StateMonad stateMonad, INuixSettings settings, CancellationToken cancellationToken)
        {
            var blockArgumentsResult = GetBlockArguments(block);

            if (blockArgumentsResult.IsFailure) return blockArgumentsResult.ConvertFailure<IReadOnlyCollection<string>>();

            var scriptTextResult = ScriptGenerator.CompileScript(block);

            if (scriptTextResult.IsFailure) return scriptTextResult.ConvertFailure<IReadOnlyCollection<string>>();

            var filePathResult = await WriteScriptToFileAsync(scriptTextResult.Value,
                stateMonad.FileSystemHelper, cancellationToken);

            if (filePathResult.IsFailure) return filePathResult.ConvertFailure<IReadOnlyCollection<string>>();

            var arguments = GetAllArguments(blockArgumentsResult.Value, settings, filePathResult.Value);

            return Result.Success<IReadOnlyCollection<string>, IErrorBuilder>(arguments);
        }


        public static async Task<Result<string, IErrorBuilder>> WriteScriptToFileAsync(string scriptText, IFileSystemHelper fileSystemHelper, CancellationToken cancellationToken)
        {
            var scriptFilePath = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), "NuixScript" + Guid.NewGuid()), "rb");
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(scriptText));

            return await fileSystemHelper.WriteFileAsync(scriptFilePath, stream, cancellationToken).Map(_ => scriptFilePath);
        }

        public static Result<IReadOnlyCollection<string>, IErrorBuilder> GetBlockArguments(IRubyBlock rubyBlock)
        {
            var suffixer = new Suffixer();
            var blockArguments = rubyBlock.TryGetArguments(suffixer);

            return blockArguments;
        }

        public static IReadOnlyCollection<string> GetAllArguments(IReadOnlyCollection<string> blockArguments,
            INuixSettings nuixSettings, string scriptFilePath)
        {
            var trueArguments = new List<string>(); //note that the arguments will be escaped in the next step
            if (nuixSettings.UseDongle)
            {
                // ReSharper disable once StringLiteralTypo
                trueArguments.Add("-licencesourcetype");
                trueArguments.Add("dongle");
            }
            trueArguments.Add(scriptFilePath);
            trueArguments.AddRange(blockArguments);

            return trueArguments;
        }

        /// <summary>
        /// Gets a string which will point to the argument value in ruby;
        /// </summary>
        public static string GetArgumentValueString(string argumentName) => $"{HashSetName}[:{argumentName}]";


    }
}