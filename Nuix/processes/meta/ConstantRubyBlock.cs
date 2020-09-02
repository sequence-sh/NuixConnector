using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using CSharpFunctionalExtensions;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;

namespace Reductech.EDR.Connectors.Nuix.processes.meta
{
    /// <summary>
    /// A ruby block representing a fixed value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class ConstantRubyBlock<T> : ITypedRubyBlock<T>
    {
        public ConstantRubyBlock(T value, string parameterName)
        {
            Value = value;
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
        public T Value { get; }
        public string Name  => $"{ParameterName}({Value})";

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <inheritdoc />
        public IEnumerable<IRubyFunction> FunctionDefinitions => ImmutableArray<IRubyFunction>.Empty;

        /// <inheritdoc />
        public Result<IReadOnlyCollection<string>, IRunErrors>  TryGetArguments(Suffixer suffixer)
        {
            var args = new List<string>
                        {
                            $"--{ParameterName}" + suffixer.GetNext(),
                            ConvertToString(Value!)
                        };

            return args;
        }

        /// <inheritdoc />
        public void WriteOptParseLines(string hashSetName, IIndentationStringBuilder sb, Suffixer suffixer)
        {
            var number = suffixer.GetNext();
            sb.AppendLine($"opts.on('--{ParameterName}{number} [ARG]') do |o| params[:{ParameterName}{number}] = o end");
        }

        /// <inheritdoc />
        public Result<string, IRunErrors> TryWriteBlockLines(Suffixer suffixer, IIndentationStringBuilder stringBuilder)
        {
            var number = suffixer.GetNext();

            var resultVariableName = ParameterName + number;
            var keyName = ParameterName + number;

            var line = $"{resultVariableName} = {RubyScriptCompilationHelper.GetArgumentValueString(keyName)}";

            stringBuilder.AppendLine(line);

            return resultVariableName;
        }


        private static string ConvertToString(object o)
        {
            return o switch
            {
                string s => s,
                bool b => b.ToString().ToLowerInvariant(),
                int i => i.ToString(),
                Enum e => e.GetDisplayName(),
                _ => o.ToString()!
            };
        }

    }
}
