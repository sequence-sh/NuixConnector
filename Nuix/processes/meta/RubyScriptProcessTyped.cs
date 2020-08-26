using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// A ruby script process that returns a particular result.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class RubyScriptProcessTyped<T> : RubyScriptProcessBase<T>
    {
        /// <summary>
        /// Gets the ruby block to run.
        /// </summary>
        private Result<ITypedRubyBlock<T>, IRunErrors> TryGetRubyBlock(ProcessState processState)
        {
            var parametersResult = TryGetMethodParameters(processState)
                .Combine(RunErrorList.Combine)
                .Map(x=>x.ToList())
                .Map(x=>
                    new BasicTypedRubyBlock<T>(
                        MethodName,
                        ScriptText, x,
                        RunTimeNuixVersion ?? RubyScriptProcessFactory.RequiredVersion,
                RubyScriptProcessFactory.RequiredFeatures) as ITypedRubyBlock<T>);

            return parametersResult;

        }



        /// <summary>
        /// Convert a string into a result of the desired type.
        /// </summary>
        public abstract bool TryParse(string s, out T result);


        /// <summary>
        /// Runs this process asynchronously
        /// </summary>
        protected override async Task<Result<T, IRunErrors>> RunAsync(ProcessState processState)
        {
            var settingsResult = processState.GetProcessSettings<INuixProcessSettings>(Name);
            if (settingsResult.IsFailure) return settingsResult.ConvertFailure<T>();


            var blockResult = TryGetRubyBlock(processState);

            if (blockResult.IsFailure)
                return blockResult.ConvertFailure<T>();


            var script = CompileScript(blockResult.Value);

            var trueArguments = await RubyScriptCompilationHelper.GetTrueArgumentsAsync(script, settingsResult.Value, new[]{blockResult.Value});

            var result = await ExternalProcessMethods.RunExternalProcess(settingsResult.Value.NuixExeConsolePath, processState.Logger,
                Name, trueArguments, TryExtractValueFromOutput);

            return result;
        }

        private string CompileScript(ITypedRubyBlock<T> block)
        {
            var scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptSetup(MethodName, new IRubyBlock[] { block, BinToHexBlock.Instance }));
            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptMethodText(new IRubyBlock[] { block, BinToHexBlock.Instance }));

            var i = 0;
            var fullMethodLine = block.GetBlockText(ref i, out var resultVariableName);

            scriptBuilder.AppendLine(fullMethodLine);


            scriptBuilder.AppendLine($"puts \"--Final Result: #{{binToHex({resultVariableName})}}\"");

            return (scriptBuilder.ToString());
        }

        /// <inheritdoc />
        public override Result<string, IRunErrors> TryCompileScript(ProcessState processState) => TryGetRubyBlock(processState).Map(CompileScript);

        // ReSharper disable once StaticMemberInGenericType
        private static readonly Regex FinalResultRegex = new Regex("--Final Result: (?<result>.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private Maybe<T> TryExtractValueFromOutput(string message)
        {
            if (FinalResultRegex.TryMatch(message, out var m))
            {
                var resultHex = m.Groups["result"].Value;

                var resultString = TryMakeStringFromHex(resultHex);
                if(resultString != null && TryParse(resultString, out var t))
                    return Maybe<T>.From(t);

            }

            return Maybe<T>.None;
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


        ///// <inheritdoc />
        //public IProcessConverter? ProcessConverter => NuixProcessConverter.Instance;

        private class BinToHexBlock : IUnitRubyBlock
        {
            public static readonly BinToHexBlock Instance = new BinToHexBlock();

            private BinToHexBlock()
            {

            }

            public string BlockName => "BinToHex";

            public Version RequiredNuixVersion { get; } = new Version(5,0);

            public IReadOnlyCollection<NuixFeature> RequiredNuixFeatures { get; } = new List<NuixFeature>();

            public IEnumerable<string> FunctionDefinitions { get; } = new List<string>()
            {
                @"def binToHex(s)
  suffix = s.to_s.each_byte.map { |b| b.to_s(16).rjust(2, '0') }.join('').upcase
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