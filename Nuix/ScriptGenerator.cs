using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using Reductech.EDR.Connectors.Nuix.processes.meta;
using Reductech.EDR.Connectors.Nuix.RubyFunctions;
using Reductech.EDR.Processes;
using Reductech.EDR.Processes.Internal;
using YamlDotNet.Serialization;

namespace Reductech.EDR.Connectors.Nuix
{
    /// <summary>
    /// Generates Nuix Ruby Scripts.
    /// </summary>
    public class ScriptGenerator
    {
        /// <summary>
        /// Create a new ScriptGenerator.
        /// </summary>
        /// <param name="nuixProcessSettings"></param>
        public ScriptGenerator(INuixProcessSettings nuixProcessSettings) => _nuixProcessSettings = nuixProcessSettings;

        private readonly INuixProcessSettings _nuixProcessSettings;

        /// <summary>
        /// Generates ruby scripts for all RubyScriptProcesses in the AppDomain.
        /// </summary>
        /// <param name="folderPath">Path to the folder to create the scripts in.</param>
        [UsedImplicitly]
        public string GenerateScripts(string folderPath)
        {
            var processTypes = AppDomain.CurrentDomain
                .GetAssemblies().SelectMany(x=>x.GetTypes())
                .Where(t => typeof(IRubyScriptProcess).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract).ToList();

            foreach (var processType in processTypes)
            {
                try
                {

                    var instance = Activator.CreateInstance(processType);

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    var process = (IRubyScriptProcess) instance;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                    if (process == null)
                        return $"Could not create process '{processType.Name}'";

                    foreach (var propertyInfo in processType.GetProperties()
                        .Where(x=>x.GetCustomAttributes(typeof(YamlMemberAttribute)).Any()))
                    {
                        var newValue =
                            propertyInfo.PropertyType == typeof(string)?
                                "value" :
                                Activator.CreateInstance(propertyInfo.PropertyType);

                        propertyInfo.SetValue(process, newValue);
                    }


                    var (isSuccess, _, value, error) = TryGenerateScript(process);
                    if (isSuccess)
                    {
                        var fileName = process.FunctionName + ".rb";
                        var newPath = Path.Combine(folderPath, fileName);

                        File.WriteAllText(newPath, value, Encoding.UTF8);

                    }
                    else
                    {
                        return error;
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
                {
                    return e.Message;
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }

            return "Scripts generated successfully";
        }


        /// <summary>
        /// The string that will be returned when the script completes successfully.
        /// </summary>
        public const string UnitSuccessToken = "--Script Completed Successfully--";


        /// <summary>
        /// Compiles a ruby script for any number of unit blocks
        /// </summary>
        public static Result<string, IRunErrors> CompileScript(string methodName, params IUnitRubyBlock[] blocks)
        {
            var scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptSetup(methodName, blocks));
            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptFunctionText(blocks));

            var suffixer = new Suffixer();

            var errors = new List<IRunErrors>();
            foreach (var rubyBlock in blocks)
            {
                var blockTextResult = rubyBlock.GetBlockText(suffixer);

                if(blockTextResult.IsFailure)
                    errors.Add(blockTextResult.Error);
                else
                    scriptBuilder.AppendLine(blockTextResult.Value);
            }

            if (errors.Any())
                return Result.Failure <string, IRunErrors>(RunErrorList.Combine(errors));

            scriptBuilder.AppendLine($"puts '{UnitSuccessToken}'");

            return (scriptBuilder.ToString());
        }


        /// <summary>
        /// Compiles a ruby script for a typed block.
        /// </summary>
        public static Result<string, IRunErrors> CompileScript<T>(string methodName, ITypedRubyBlock<T> block)
        {
            var bb = new TypedCompoundRubyBlock<string>(BinToHexFunction.Instance,
                new Dictionary<RubyFunctionParameter, ITypedRubyBlock>()
                {
                    {
                        BinToHexFunction.Instance.Arguments.Single(),
                        block
                    }
                }
            );


            var blocks = new[] {bb};

            var scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptSetup(methodName,  blocks));
            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptFunctionText(blocks));


            var fullMethodLineResult = bb.GetBlockText(new Suffixer(), out var resultVariableName);

            if (fullMethodLineResult.IsFailure)
                return fullMethodLineResult.ConvertFailure<string>();

            scriptBuilder.AppendLine(fullMethodLineResult.Value);


            //scriptBuilder.AppendLine($"puts \"--Final Result: #{{binToHex({resultVariableName})}}\"");
            scriptBuilder.AppendLine($"puts \"--Final Result: #{{{resultVariableName}}}\"");

            return scriptBuilder.ToString();
        }

        private Result<string> TryGenerateScript(IRubyScriptProcess process)
        {
            var state = new ProcessState(NullLogger.Instance, _nuixProcessSettings, ExternalProcessRunner.Instance);

            var result = process.TryCompileScript(state).MapFailure(x=>x.AsString);
            return result;

        }
    }
}
