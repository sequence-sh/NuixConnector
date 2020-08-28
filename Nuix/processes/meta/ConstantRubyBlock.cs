using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// A ruby block representing a fixed value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class ConstantRubyBlock<T> : ITypedRubyBlock<T>
    {
        public ConstantRubyBlock(T value) => Value = value;

        public T Value { get; }

        /// <inheritdoc />
        public override string ToString() => $"Argument({Value})";

        /// <inheritdoc />
        public IEnumerable<IRubyFunction> FunctionDefinitions => ImmutableArray<IRubyFunction>.Empty;

        /// <inheritdoc />
        public Result<IReadOnlyCollection<string>, IRunErrors>  TryGetArguments(Suffixer suffixer)
        {
            var args = new List<string>
                        {
                            $"--{ArgPrefix}" + suffixer.GetNext(),
                            Value!.ToString()!
                        };

            return args;
        }

        private const string VarPrefix = "value";
        private const string KeyPrefix = "key";
        private const string ArgPrefix = "arg";

        /// <inheritdoc />
        public IReadOnlyCollection<string> GetOptParseLines(string hashSetName, Suffixer suffixer)
        {
            var number = suffixer.GetNext();

            var lines = new List<string>
                        {
                            $"opts.on('--{ArgPrefix}{number} [ARG]') do |o| params[:{KeyPrefix}{number}] = o end",
                        };

            return lines;
        }

        /// <inheritdoc />
        public Result<string, IRunErrors> GetBlockText(Suffixer suffixer, out string resultVariableName)
        {
            var number = suffixer.GetNext();

            resultVariableName = VarPrefix + number;
            var keyName = KeyPrefix + number;

            var line = $"{resultVariableName} = {RubyScriptCompilationHelper.GetArgumentValueString(keyName)}";

            return line;
        }
    }
}
