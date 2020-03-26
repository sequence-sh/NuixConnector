using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.immutable;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    //TODO seal this class and get rid of virtual methods - they do not work with composition
    internal class ImmutableRubyScriptProcess : ImmutableProcess
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

            var printArguments = false;

#if DEBUG
            printArguments = true;
#endif
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

            return (sb.ToString(), arguments);
        }


        /// <inheritdoc />
        public override async IAsyncEnumerable<Result<string>> Execute()
        {
            var processState = new ProcessState();

            foreach (var lb in BeforeScriptStart(processState))
                yield return lb;

            var (scriptText, arguments) = CompileScript();
            var scriptFilePath = Path.ChangeExtension(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()), "rb");
            
            await System.IO.File.WriteAllTextAsync(scriptFilePath, scriptText);

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
            //TODO this DOES NOT WORK with derived methods - get rid of them!

            if (CanBeCombined &&
                nextProcess is ImmutableRubyScriptProcess np 
                && np.CanBeCombined
                && _nuixExeConsolePath == np._nuixExeConsolePath && _useDongle == np._useDongle)
            {
                var newProcess = new ImmutableRubyScriptProcess(
                    $"{Name} then {np.Name}", 
                    _nuixExeConsolePath, 
                    _useDongle,
                    _methodSet.Concat(np._methodSet).Distinct().ToList(),
                    _methodCalls.Concat(np._methodCalls).ToList()
                    );

                return Result.Success<ImmutableProcess>(newProcess);
            }

            return Result.Failure<ImmutableProcess>("Could not combine");
        }

        /// <summary>
        /// Do something with a line returned from the script
        /// </summary>
        /// <param name="rl">The line to look at</param>
        /// <param name="processState">The current state of the process</param>
        /// <returns>True if the line should continue through the pipeline</returns>
        internal virtual bool HandleLine(Result<string> rl, ProcessState processState)
        {
            return true;
        }

        /// <summary>
        /// What to do before the script starts.
        /// </summary>
        internal virtual IEnumerable<Result<string>> BeforeScriptStart(ProcessState processState)
        {
            yield break;
        }

        /// <summary>
        /// What to do when the script finishes
        /// </summary>
        internal virtual IEnumerable<Result<string>> OnScriptFinish(ProcessState processState)
        {
            yield break;
        }

        internal virtual bool CanBeCombined => true;

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