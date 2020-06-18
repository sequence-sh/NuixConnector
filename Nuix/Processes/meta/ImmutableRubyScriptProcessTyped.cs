using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.output;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    internal sealed class ImmutableRubyScriptProcessTyped<T> : ImmutableProcess<T>, IImmutableRubyScriptProcess
    {
        public readonly ITypedRubyBlock<T> RubyBlock;
        private readonly INuixProcessSettings _nuixProcessSettings;
        public readonly Func<string, Result<T>> TryParseFunc;

        /// <inheritdoc />
        public ImmutableRubyScriptProcessTyped( ITypedRubyBlock<T> rubyBlock, INuixProcessSettings nuixProcessSettings, Func<string, Result<T>> tryParseFunc)

        {
            RubyBlock = rubyBlock;
            _nuixProcessSettings = nuixProcessSettings;
            TryParseFunc = tryParseFunc;
        }

        /// <inheritdoc />
        public override string Name => RubyBlock.BlockName;

        /// <inheritdoc />
        public override async IAsyncEnumerable<IProcessOutput<T>> Execute()
        {
            var scriptText = CompileScript();
            var trueArguments = await RubyScriptCompilationHelper.GetTrueArgumentsAsync(scriptText, _nuixProcessSettings, new []{RubyBlock});

            IProcessOutput<T>? successOutput = null;

            await foreach (var output in ExternalProcessHelper.RunExternalProcess(_nuixProcessSettings.NuixExeConsolePath, trueArguments))
            {
                if (output.OutputType == OutputType.Message && TryExtractValueFromOutput(output.Text, out var val))
                    successOutput = ProcessOutput<T>.Success(val);
                else if (output.OutputType == OutputType.Error && RubyScriptCompilationHelper.NuixWarnings.Contains(output.Text))
                {
                    yield return ProcessOutput<T>.Warning(output.Text);
                }
                else
                    yield return output.ConvertTo<T>();
            }

            if (successOutput != null)
                yield return successOutput;
            else
                yield return ProcessOutput<T>.Error("Process did not complete successfully");
        }

        public string CompileScript()
        {
            var scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptSetup(Name, new IRubyBlock []{RubyBlock, BinToHexBlock.Instance}));
            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptMethodText(new IRubyBlock[]{RubyBlock, BinToHexBlock.Instance}));

            var i = 0;
            var fullMethodLine = RubyBlock.GetBlockText(ref i, out var resultVariableName);

            scriptBuilder.AppendLine(fullMethodLine);


            scriptBuilder.AppendLine($"puts \"--Final Result: #{{binToHex({resultVariableName})}}\"");

            return (scriptBuilder.ToString());
        }

        // ReSharper disable once StaticMemberInGenericType
        private static readonly Regex FinalResultRegex = new Regex("--Final Result: (?<result>.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private bool TryExtractValueFromOutput(string message, out T value)
        {
            if (FinalResultRegex.TryMatch(message, out var m))
            {
                var resultHex = m.Groups["result"].Value;

                var resultString = TryMakeStringFromHex(resultHex);
                if(resultString != null)
                {
                    var (isSuccess, _, value1) = TryParseFunc(resultString);
                    if (isSuccess)
                    {
                        value = value1;
                        return true;
                    }
                }

            }

#pragma warning disable CS8601 // Possible null reference assignment.
            value = default;
#pragma warning restore CS8601 // Possible null reference assignment.
            return false;
        }


        private static string? TryMakeStringFromHex(string hexString)
        {
            if(hexString.StartsWith("0x"))
            {
                hexString = hexString.Substring(2);//ignore the 0x

                var bytes = new byte[hexString.Length  / 2];
                try
                {
                    for (var i = 0; i < bytes.Length; i++) bytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (FormatException)
                {
                    return null;//Failed
                }
#pragma warning restore CA1031 // Do not catch general exception types

                return Encoding.UTF8.GetString(bytes);
            }
            return null;
        }



        /// <inheritdoc />
        public override IProcessConverter? ProcessConverter => NuixProcessConverter.Instance;

        private class BinToHexBlock : IUnitRubyBlock
        {
            public static BinToHexBlock Instance = new BinToHexBlock();

            private BinToHexBlock()
            {

            }

            public string BlockName => "BinToHex";

            public Version RequiredNuixVersion { get; } = new Version(5,0);

            public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures { get; } = new List<NuixFeature>();

            public IEnumerable<string> FunctionDefinitions { get; } = new List<string>()
            {
                @"def binToHex(s)
  suffix = s.each_byte.map { |b| b.to_s(16).rjust(2, '0') }.join('').upcase
  '0x' + suffix
end
"
            };

            public IReadOnlyCollection<string> GetArguments(ref int blockNumber)
            {
                return new List<string>();
            }

            public string GetBlockText(ref int blockNumber)
            {
                return string.Empty;
            }

            public IReadOnlyCollection<string> GetOptParseLines(string hashSetName, ref int blockNumber)
            {
                return new List<string>();
            }
        }
    }
}