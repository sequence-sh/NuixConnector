using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.immutable;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    internal sealed class ImmutableRubyScriptProcess : ImmutableProcess
    {
        /// <inheritdoc />
        public ImmutableRubyScriptProcess(
            string name, 
            string nuixExeConsolePath,
            bool useDongle,
            IReadOnlyCollection<string> methodSet, 
            IReadOnlyCollection<MethodCall> methodCalls) : base(name)
        {
            _nuixExeConsolePath = nuixExeConsolePath;
            _useDongle = useDongle;
            _methodSet = methodSet;
            _methodCalls = methodCalls;
        }

        private readonly string _nuixExeConsolePath;
        private readonly bool _useDongle;

        /// <summary>
        /// All the ruby methods that might be used
        /// </summary>
        private readonly IReadOnlyCollection<string> _methodSet;


        private readonly IReadOnlyCollection<MethodCall> _methodCalls;

        private (string scriptText, IReadOnlyCollection<string> arguments) CompileScript()
        {
            var sb = new StringBuilder();

            // ReSharper disable once JoinDeclarationAndInitializer
            bool printArguments;

#if DEBUG
            printArguments = true;
#endif
            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            if(printArguments)
                sb.AppendLine("puts ARGV.join('; ')");


            foreach (var method in _methodSet)
            {
                sb.AppendLine(method);
                sb.AppendLine();
            }

            var arguments = new List<string>();

            foreach (var methodCall in _methodCalls)
            {
                var callLine = new StringBuilder(methodCall.MethodName + "(utilities"); //utilities is always first argument

                foreach (var (_, value) in methodCall.MethodParameters)
                {
                    callLine.Append(", ");
                    if (value == null) callLine.Append("nil");
                    else
                    {
                        callLine.Append($"ARGV[{arguments.Count}]");
                        arguments.Add(value);
                    }
                }

                callLine.Append(")");

                sb.AppendLine(callLine.ToString());
            }

            sb.AppendLine($"puts '{SuccessToken}'");

            return (sb.ToString(), arguments);
        }

        public const string SuccessToken = "--Script Completed Successfully--";


        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute()
        {
            var processState = new ProcessState();

            var (scriptText, arguments) = CompileScript();
            var scriptFilePath = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), "NuixScript" + Guid.NewGuid().ToString()), "rb");
            
            await File.WriteAllTextAsync(scriptFilePath, scriptText);

            var trueArguments = new List<string>(); //note that the arguments will be escaped in the next step
            if (_useDongle)
            {
                // ReSharper disable once StringLiteralTypo
                trueArguments.Add("-licencesourcetype");
                trueArguments.Add("dongle");  
            }
            trueArguments.Add(scriptFilePath);
            trueArguments.AddRange(arguments);

            await foreach (var rl in ExternalProcessHelper.RunExternalProcess(_nuixExeConsolePath, trueArguments))
            {
                if(HandleLine(rl, processState))
                    yield return rl;
            }

            foreach (var l in OnScriptFinish(processState))
                yield return l;
        }        

        /// <inheritdoc />
        public override Result<ImmutableProcess> TryCombine(ImmutableProcess nextProcess)
        {
            if (!(nextProcess is ImmutableRubyScriptProcess np) || _nuixExeConsolePath != np._nuixExeConsolePath ||
                _useDongle != np._useDongle) return Result.Failure<ImmutableProcess>("Could not combine");

            var newProcess = new ImmutableRubyScriptProcess(
                $"{Name} then {np.Name}", 
                _nuixExeConsolePath, 
                _useDongle,
                _methodSet.Concat(np._methodSet).Distinct().ToList(),
                _methodCalls.Concat(np._methodCalls).ToList()
            );

            return Result.Success<ImmutableProcess>(newProcess);
        }

        /// <summary>
        /// Do something with a line returned from the script
        /// </summary>
        /// <param name="rl">The line to look at</param>
        /// <param name="processState">The current state of the process</param>
        /// <returns>True if the line should continue through the pipeline</returns>
        internal bool HandleLine(Result<string> rl, ProcessState processState)
        {
            var (isSuccess, _, value) = rl;
            if (!isSuccess || value != SuccessToken) return true;
            processState.Artifacts.Add(SuccessToken, true);
            return false;
        }

        /// <summary>
        /// What to do when the script finishes
        /// </summary>
        internal IEnumerable<Result<string>> OnScriptFinish(ProcessState processState)
        {
            if (!(processState.Artifacts.TryGetValue(SuccessToken, out var s) && s is bool b && b))
                yield return Result.Failure<string>("Process did not complete successfully");
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object? obj)
        {
            if (!(obj is ImmutableRubyScriptProcess rsp))
                return false;

            return Name == rsp.Name && _nuixExeConsolePath.Equals(rsp._nuixExeConsolePath) &&
                   _methodCalls.SequenceEqual(rsp._methodCalls);
        }

        public class MethodCall
        {
            /// <inheritdoc />
            public override string ToString()
            {
                return MethodName;
            }

            /// <summary>
            /// The name of the method.
            /// </summary>
            public readonly string MethodName;

            /// <summary>
            /// The parameters to send to the method
            /// </summary>
            public readonly IReadOnlyList<KeyValuePair<string, string?>> MethodParameters;

            public MethodCall(string methodName, IEnumerable<KeyValuePair<string, string?>> methodParameters)
            {
                MethodName = methodName;
                MethodParameters = methodParameters.ToList();
            }

            /// <inheritdoc />
            public override int GetHashCode()
            {
                return HashCode.Combine(MethodName);
            }
        }
    }
}