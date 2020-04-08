using System;
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
        public string GenerateScripts(string folderPath)
        {
            var processTypes = AppDomain.CurrentDomain
                .GetAssemblies().SelectMany(x=>x.GetTypes())
                .Where(t => typeof(RubyScriptProcess).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract).ToList();

            //var folderPath = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.Parent.FullName;

            foreach (var processType in processTypes)
            {
                try
                {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
                    var process = (RubyScriptProcess)Activator.CreateInstance(processType);
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
                        var fileName = process.MethodName + ".rb";
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
