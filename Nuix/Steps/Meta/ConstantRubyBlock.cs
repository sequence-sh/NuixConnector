using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CSharpFunctionalExtensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
using Entity = Reductech.EDR.Core.Entities.Entity;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    /// <summary>
    /// A ruby block representing a fixed value
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal sealed class ConstantRubyBlock<T> : ITypedRubyBlock<T>
    {
        public ConstantRubyBlock(T value, string parameterName)
        {
            Value = value!;
            ParameterName = parameterName;
        }

        public ConstantRubyBlock(string parameterName)
        {
            Value = Maybe<T>.None;
            ParameterName = parameterName;
        }

        public string ParameterName { get; }
        public Maybe<T> Value { get; }
        public string Name  => $"{ParameterName}({Value.Value })";

        /// <inheritdoc />
        public override string ToString() => Name;

        /// <inheritdoc />
        public IEnumerable<IRubyFunction> FunctionDefinitions => ImmutableArray<IRubyFunction>.Empty;

        /// <inheritdoc />
        public Result<IReadOnlyCollection<string>, IErrorBuilder>  TryGetArguments(Suffixer suffixer)
        {
            List<string> args;

            if(Value.HasValue)
            {
                args = new List<string>
                        {
                            $"--{ParameterName}" + suffixer.CurrentSuffix,
                            ConvertToString(Value.Value!)
                        };
            }
            else
            {
                args = new List<string>();
            }



            return args;
        }

        /// <inheritdoc />
        public void WriteOptParseLines(string hashSetName, IIndentationStringBuilder sb, Suffixer suffixer)
        {
            var number = suffixer.GetNext();
            sb.AppendLine($"opts.on('--{ParameterName}{number} ARG') do |o| params[:{ParameterName}{number}] = o end");
        }

        /// <inheritdoc />
        public Result<string, IErrorBuilder> TryWriteBlockLines(Suffixer suffixer, IIndentationStringBuilder stringBuilder)
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
                double d => d.ToString("G"),
                Enum e => e.GetDisplayName(),
                Entity entity => SerializeJson(entity),
                IEnumerable enumerable => "[" + string.Join(", ", enumerable.Cast<object>().Select(ConvertToString)) + "]",
            _ => throw new ArgumentException($"Cannot convert '{o.GetType().Name}' to string", nameof(o))
            };
        }

        private static string SerializeJson(Entity entity)
        {
            JObject jObject = new JObject();

            foreach (var (key, value) in entity)
            {
                value.Value.Switch(
                    _=>{},
                    x=> jObject.Add(key, GetJToken(x)),
                    x => jObject.Add(key, GetJArray(x)));
            }

            var s = JsonConvert.SerializeObject(jObject);

            s = "'" + s.Replace("'", "''") + "'";

            return s;

            static JArray GetJArray(IReadOnlyCollection<EntitySingleValue> list)
            {
                var array = new JArray();

                foreach (var entitySingleValue in list) array.Add(GetJToken(entitySingleValue));

                return array;
            }

            static JToken GetJToken(EntitySingleValue esv)
            {
                JToken jToken = null!;


                 esv.Value.Switch(
                        a=> jToken = a,
                        a=> jToken = a,
                        a=> jToken = a,
                        a=> jToken = a,
                        a=> jToken = a,
                        a=> jToken = a
                        );

                 return jToken!; //Will be set by the switch statement
            }
        }

    }
}