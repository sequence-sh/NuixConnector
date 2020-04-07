using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CSharpFunctionalExtensions;
using JetBrains.Annotations;
using Reductech.EDR.Connectors.Nuix.processes.meta;
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
        public ScriptGenerator(INuixProcessSettings nuixProcessSettings)
        {
            _nuixProcessSettings = nuixProcessSettings;
        }

        private readonly INuixProcessSettings _nuixProcessSettings;

        /// <summary>
        /// Generates ruby scripts for all RubyScriptProcesses in the AppDomain.
        /// </summary>
        /// <param name="folderPath">Path to the folder to create the scripts in.</param>
        [UsedImplicitly]
        public Result GenerateScripts(string folderPath)
        {
            var processTypes = AppDomain.CurrentDomain
                .GetAssemblies().SelectMany(x=>x.GetTypes())
                .Where(t => typeof(RubyScriptProcess).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract).ToList();

            foreach (var processType in processTypes)
            {
                RubyScriptProcess process;

                try
                {
                    process = (RubyScriptProcess)Activator.CreateInstance(processType);
                }
                catch (Exception e)
                {
                    return Result.Failure(e.Message);
                }


                foreach (var propertyInfo in processType.GetProperties()
                    .Where(x=>x.GetCustomAttributes(typeof(YamlMemberAttribute)).Any()))
                {
                    try
                    {
                        var newValue = 
                            propertyInfo.PropertyType == typeof(string)?
                                "value" :
                                Activator.CreateInstance(propertyInfo.PropertyType);

                        propertyInfo.SetValue(process, newValue);
                    }
                    catch (Exception e)
                    {
                        return Result.Failure(e.Message);
                    }
                }

                
                var scriptResult = TryGenerateScript(process);
                if (scriptResult.IsSuccess)
                {
                    var fileName = process.MethodName + ".rb";
                    var newPath = Path.Combine(folderPath, fileName);

                    try
                    {
                        File.WriteAllText(newPath, scriptResult.Value, Encoding.UTF8);
                    }
                    catch (Exception e)
                    {
                        return Result.Failure(e.Message);
                    }
                    
                }
                else
                {
                    return scriptResult.ConvertFailure();
                }
            }

            return Result.Success();
        }

        private Result<string> TryGenerateScript(RubyScriptProcess process)
        {
            var freezeResult =process.TryFreeze(_nuixProcessSettings);

            if (freezeResult.IsFailure)
                return freezeResult.ConvertFailure<string>();

            if (freezeResult.Value is IImmutableRubyScriptProcess immutableRubyScriptProcess)
            {
                var script = immutableRubyScriptProcess.CompileScript();
                return Result.Success(script);
            }
            else
            {
                return Result.Failure<string>("Could not cast process to IImmutableRubyScriptProcess");
            }

        }

    }
}
