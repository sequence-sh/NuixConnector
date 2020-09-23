using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using Reductech.EDR.Connectors.Nuix.Processes.Meta;
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
                        return error;
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
        public static Result<string, IRunErrors> CompileScript(IUnitRubyBlock block)
        {
            var scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptSetup(block));
            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptFunctionText(block));

            var suffixer = new Suffixer();

            var blockTextResult = block.TryWriteBlockLines(suffixer, new IndentationStringBuilder(scriptBuilder, 0));

            if (blockTextResult.IsFailure)
                return blockTextResult.ConvertFailure<string>();

            scriptBuilder.AppendLine($"puts '{UnitSuccessToken}'");

            return (scriptBuilder.ToString());
        }


        /// <summary>
        /// Compiles a ruby script for a typed block.
        /// </summary>
        public static Result<string, IRunErrors> CompileScript<T>(ITypedRubyBlock<T> block)
        {
            var compoundRubyBlock = new TypedCompoundRubyBlock<string>(BinToHexFunction.Instance,
                new Dictionary<RubyFunctionParameter, ITypedRubyBlock>
                {
                    {
                        BinToHexFunction.Instance.Arguments.Single(),
                        block
                    }
                });


            var scriptBuilder = new StringBuilder();

            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptSetup( compoundRubyBlock));
            scriptBuilder.AppendLine(RubyScriptCompilationHelper.CompileScriptFunctionText(compoundRubyBlock));


            var fullMethodLineResult = compoundRubyBlock.TryWriteBlockLines(new Suffixer(), new IndentationStringBuilder(scriptBuilder, 0));

            if (fullMethodLineResult.IsFailure)
                return fullMethodLineResult;

            scriptBuilder.AppendLine(fullMethodLineResult.Value);

            scriptBuilder.AppendLine($"puts \"--Final Result: #{{{fullMethodLineResult.Value}}}\"");

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
