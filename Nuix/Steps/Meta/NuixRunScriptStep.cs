using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Parser;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{
    /// <summary>
    /// Run an arbitrary ruby script in nuix.
    /// It should return a string.
    /// </summary>
    public class NuixRunScript : CompoundStep<StringStream>
    {
        /// <inheritdoc />
        public override async Task<Result<StringStream, IError>> Run(IStateMonad stateMonad, CancellationToken cancellationToken)
        {
            //var isAdmin = IsCurrentProcessAdmin();
            //var isLinux = IsLinux;
            //if(isAdmin &&!isLinux)
            //    return new SingleError("You cannot run arbitrary Nuix Scripts as Administrator", ErrorCode.ExternalProcessError, new StepErrorLocation(this));

            var functionName = await FunctionName.Run(stateMonad, cancellationToken)
                    .Map(x=>x.GetStringAsync())
                ;
            if (functionName.IsFailure) return functionName.ConvertFailure<StringStream>();

            var scriptText = await ScriptText.Run(stateMonad, cancellationToken)
                .Map(x => x.GetStringAsync());

            if (scriptText.IsFailure) return scriptText.ConvertFailure<StringStream>();

            var parameters = await Parameters.Run(stateMonad, cancellationToken);
            if (parameters.IsFailure) return parameters.ConvertFailure<StringStream>();

            var nuixConnection = stateMonad.GetOrCreateNuixConnection(false);
            if (nuixConnection.IsFailure) return nuixConnection.ConvertFailure<StringStream>().MapError(x => x.WithLocation(this));



            var rubyFunctionParameters = parameters.Value
                .Select(x => new RubyFunctionParameter(ConvertString(x.Key), x.Key, true, null))
                .ToList();

            var parameterDict = parameters.Value
                .ToDictionary(x =>
                        new RubyFunctionParameter( ConvertString(x.Key), x.Key, true, null),
                    x=>GetObject(x.Value))
                .Where(x=>x.Value != null)
                .ToDictionary(x=>x.Key, x=>x.Value!);

            if (EntityStreamParameter != null)
            {
                var streamResult = await EntityStreamParameter.Run(stateMonad, cancellationToken);
                if (streamResult.IsFailure) return streamResult.ConvertFailure<StringStream>();

                const string streamParameter = "datastream";

                var parameter = new RubyFunctionParameter(ConvertString(streamParameter), streamParameter, true, null);

                rubyFunctionParameters.Add(parameter);

                parameterDict.Add(parameter, streamResult.Value);
            }
            var function = new RubyFunction<string>(ConvertString(functionName.Value), scriptText.Value, rubyFunctionParameters);

            var runResult = await nuixConnection.Value.RunFunctionAsync(stateMonad.Logger, function, parameterDict, cancellationToken)
                .Map(x=> new StringStream(x));

            if (runResult.IsFailure)
                return runResult.MapError(x=>x.WithLocation(this)).ConvertFailure<StringStream>();


            return runResult.Value;

            static object? GetObject(EntityValue entityValue)
            {
                return
                entityValue.Value.Match(_ => null as object,
                    GetObject,
                    l=> l.Select(GetObject).ToList()
                );

                static object GetObject(EntitySingleValue entitySingleValue)
                {
                    return entitySingleValue.Value.Match(
                        x => x,
                        x => x,
                        x => x,
                        x => x,
                        x => x,
                        x => x as object,
                        x => x

                    );
                }

            }

        }

        private static string ConvertString(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;

            s = s.Replace(' ', '_');

            if (char.IsUpper(s.First()))
            {
                s = char.ToLowerInvariant(s[0]) + s.Substring(1);
            }

            return s;
        }

        /// <summary>
        /// What to call this function.
        /// This will have spaces replaced with underscores and the first character will be made lowercase
        /// </summary>
        [StepProperty(1)]
        [Required]
        public IStep<StringStream> FunctionName { get; set; } = null!;

        /// <summary>
        /// The text of the script to run
        /// </summary>
        [StepProperty( 2)]
        [Required]
        public IStep<StringStream> ScriptText { get;set; } = null!;

        /// <summary>
        /// Parameters to send to the script.
        /// You can access these by name (with spaces replaced by underscores and the first character lowercase).
        /// </summary>
        [StepProperty( 3)]
        [Required]
        public IStep<Entity> Parameters { get;set; }= null!;

        /// <summary>
        /// Entities to stream to the script.
        /// The parameter name is 'entityStream'
        /// </summary>
        [StepProperty( 4)]
        [DefaultValueExplanation("Do not stream entities")]
        public IStep<EntityStream>? EntityStreamParameter { get;set; }= null!;


        //private static bool IsCurrentProcessAdmin()
        //{
        //    using var identity = System.Security.Principal.WindowsIdentity.GetCurrent();
        //    var principal = new System.Security.Principal.WindowsPrincipal(identity);
        //    return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
        //}

        //public static bool IsLinux
        //{
        //    get
        //    {
        //        var p = (int)Environment.OSVersion.Platform;
        //        return (p == 4) || (p == 6) || (p == 128);
        //    }
        //}


        /// <inheritdoc />
        public override IStepFactory StepFactory => NuixRunScriptStepFactory.Instance;
    }

    /// <summary>
    /// Run an arbitrary ruby script in nuix.
    /// It should return a string.
    /// </summary>
    public class NuixRunScriptStepFactory : SimpleStepFactory<NuixRunScript, StringStream>
    {
        private NuixRunScriptStepFactory() {}

        /// <summary>
        /// The instance.
        /// </summary>
        public static SimpleStepFactory<NuixRunScript, StringStream> Instance { get; } = new NuixRunScriptStepFactory();

        /// <inheritdoc />
        public override IEnumerable<Requirement> Requirements
        {
            get
            {
                yield return new Requirement
                {
                    MinVersion = new Version(8,2),
                    Name = RubyScriptStepBase<object>.NuixRequirementName
                };
            }

        }
    }

}