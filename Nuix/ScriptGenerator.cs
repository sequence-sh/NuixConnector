using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging.Abstractions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Connectors.Nuix.RubyFunctions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;
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
        /// <param name="nuixSettings"></param>
        public ScriptGenerator(INuixSettings nuixSettings) => _nuixSettings = nuixSettings;

        private readonly INuixSettings _nuixSettings;

        /// <summary>
        /// Generates ruby scripts for all RubyScriptProcesses in the AppDomain.
        /// </summary>
        /// <param name="folderPath">Path to the folder to create the scripts in.</param>
        /// <param name="cancellationToken"></param>
        [UsedImplicitly]
        public async Task<string> GenerateScriptsAsync(string folderPath, CancellationToken cancellationToken)
        {
            var processTypes = AppDomain.CurrentDomain
                .GetAssemblies().SelectMany(x=>x.GetTypes())
                .Where(t => typeof(IRubyScriptStep).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract).ToList();

            var factoryStore = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(IRubyScriptStep));

            foreach (var processType in processTypes)
            {
                try
                {
                    var instance = Activator.CreateInstance(processType);

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    var process = (IRubyScriptStep) instance;
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

                    if (process == null)
                        return $"Could not create step '{processType.Name}'";

                    foreach (var propertyInfo in processType.GetProperties()
                        .Where(x=>x.GetCustomAttributes(typeof(YamlMemberAttribute)).Any()))
                    {
                        var newValue =
                            propertyInfo.PropertyType == typeof(string)?
                                "value" :
                                Activator.CreateInstance(propertyInfo.PropertyType);

                        propertyInfo.SetValue(process, newValue);
                    }


                    var (isSuccess, _, value, error) = await TryGenerateScript(process, factoryStore, cancellationToken);
                    if (isSuccess)
                    {
                        var fileName = process.FunctionName + ".rb";
                        var newPath = Path.Combine(folderPath, fileName);

                        await File.WriteAllTextAsync(newPath, value, Encoding.UTF8, cancellationToken);

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
        /// Compiles a ruby script for a ruby block
        /// </summary>
        public static Result<string, IErrorBuilder> CompileScript(IRubyBlock block)
        {
            return block switch
            {
                IUnitRubyBlock unitRubyBlock => CompileScript(unitRubyBlock),
                ITypedRubyBlock typedRubyBlock => CompileScript(typedRubyBlock),
                _ => throw new SystemException("Block was not a unit block or a typed block")
            };
        }

        /// <summary>
        /// Compiles a ruby script for any number of unit blocks
        /// </summary>
        public static Result<string, IErrorBuilder> CompileScript(IUnitRubyBlock block)
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
        public static Result<string, IErrorBuilder> CompileScript(ITypedRubyBlock block)
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

        private Task<Result<string>> TryGenerateScript(IRubyScriptStep step, StepFactoryStore factoryStore, CancellationToken cancellationToken)
        {
            var state = new StateMonad(NullLogger.Instance, _nuixSettings, ExternalProcessRunner.Instance, FileSystemHelper.Instance, factoryStore );

            var result = step.TryCompileScriptAsync(state, cancellationToken).MapError(x=>x.AsString);
            return result;

        }
    }
}
