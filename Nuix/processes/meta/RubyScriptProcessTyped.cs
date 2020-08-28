using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
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
        private Result<ITypedRubyBlock<T>, IRunErrors> TryGetRubyBlock(ProcessState processState) =>
            TryGetMethodParameters(processState)
                .Map(x =>
                    new TypedFunctionRubyBlock<T>(RubyScriptProcessFactory.RubyFunction, x) as ITypedRubyBlock<T>);


        /// <inheritdoc />
        public override Result<string, IRunErrors> TryCompileScript(ProcessState processState) => TryGetRubyBlock(processState)
            .Bind(b => ScriptGenerator.CompileScript(RubyScriptProcessFactory.RubyFunction.FunctionName, b));

        /// <inheritdoc />
        public override Result<IRubyBlock> TryConvert() =>
            TryGetArgumentsAsFunctions()
                .Map(args=>new TypedCompoundRubyBlock<T>(RubyScriptProcessFactory.RubyFunction, args) as IRubyBlock);


        /// <summary>
        /// Runs this process asynchronously
        /// </summary>
        protected override async Task<Result<T, IRunErrors>> RunAsync(ProcessState processState)
        {
            var settingsResult = processState.GetProcessSettings<INuixProcessSettings>(Name);
            if (settingsResult.IsFailure)
                return settingsResult.ConvertFailure<T>();


            ITypedRubyBlock<T> block;


            var argsAsFunctionsResult = TryGetArgumentsAsFunctions();
            if (argsAsFunctionsResult.IsSuccess)
            {
                block = new TypedCompoundRubyBlock<T>(RubyScriptProcessFactory.RubyFunction, argsAsFunctionsResult.Value);
            }
            else
            {
                var blockResult = TryGetRubyBlock(processState);//This will run child functions
                if (blockResult.IsFailure) return blockResult.ConvertFailure<T>();
                block = blockResult.Value;

            }

            var argumentsResult = ScriptGenerator.CompileScript(Name, block)
                    .Bind(st => RubyScriptCompilationHelper.TryGetTrueArgumentsAsync(st, settingsResult.Value, block)).Result;

            if (argumentsResult.IsFailure)
                return argumentsResult.ConvertFailure<T>();

            var scriptProcessLogger = new ScriptProcessLogger(processState, GetMaybe);


            var result = await ExternalProcessMethods.RunExternalProcess(settingsResult.Value.NuixExeConsolePath,
                scriptProcessLogger,
                Name, argumentsResult.Value);

            if (result.IsFailure) return result.ConvertFailure<T>();

            if (scriptProcessLogger.FinalOutput.HasValue)
                return scriptProcessLogger.FinalOutput.Value!;

            return new RunError("No value was returned from nuix function", Name, null, ErrorCode.ExternalProcessMissingOutput);
        }



        /// <summary>
        /// Convert a string into a result of the desired type.
        /// </summary>
        public abstract bool TryParse(string s, out T result);

        private Maybe<T> GetMaybe(string s)
        {
            if (TryParse(s, out var r))
                return Maybe<T>.From(r);

            return Maybe<T>.None;
        }


        internal sealed class ScriptProcessLogger : ILogger
        {
            public ScriptProcessLogger(ProcessState processState, Func<string, Maybe<T>> tryParseFunc)
            {
                ProcessState = processState;
                TryParseFunc = tryParseFunc;
            }

            public ProcessState ProcessState { get; }
            public Func<string, Maybe<T>> TryParseFunc { get; }

            public Maybe<T> FinalOutput { get; private set; }


            /// <inheritdoc />
            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                var m = TryExtractValueFromOutput(state?.ToString());

                if (m.HasValue)
                    FinalOutput = m;
                else
                    ProcessState.Logger.Log(logLevel, eventId, state, exception, formatter);
            }

            /// <inheritdoc />
            public bool IsEnabled(LogLevel logLevel) => ProcessState.Logger.IsEnabled(logLevel);

            /// <inheritdoc />
            public IDisposable BeginScope<TState>(TState state) => ProcessState.Logger.BeginScope(state);



            // ReSharper disable once StaticMemberInGenericType
            private static readonly Regex FinalResultRegex = new Regex("--Final Result: (?<result>.+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            private Maybe<T> TryExtractValueFromOutput(string? message)
            {
                if (message == null) return Maybe<T>.None;

                if (!FinalResultRegex.TryMatch(message, out var m)) return Maybe<T>.None;

                var resultHex = m.Groups["result"].Value;

                var resultString = TryMakeStringFromHex(resultHex);

                if (resultString != null)
                    return TryParseFunc(resultString);
                return Maybe<T>.None;
            }


            private static string? TryMakeStringFromHex(string hexString)
            {
                if (hexString.StartsWith("0x"))
                {
                    hexString = hexString.Substring(2);//ignore the 0x

                    var bytes = new byte[hexString.Length / 2];
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
        }
    }
}