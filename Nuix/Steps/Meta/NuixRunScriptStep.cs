using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Reductech.Sequence.Core.Internal.Errors;
using Entity = Reductech.Sequence.Core.Entity;

namespace Reductech.Sequence.Connectors.Nuix.Steps.Meta;

/// <summary>
/// Run an arbitrary ruby script in nuix.
/// It should return a string.
/// </summary>
public class NuixRunScript : CompoundStep<StringStream>
{
    /// <inheritdoc />
    protected override async Task<Result<StringStream, IError>> Run(
        IStateMonad stateMonad,
        CancellationToken cancellationToken)
    {
        //var isAdmin = IsCurrentProcessAdmin();
        //var isLinux = IsLinux;
        //if(isAdmin &&!isLinux)
        //    return new SingleError("You cannot run arbitrary Nuix Scripts as Administrator", ErrorCode.ExternalProcessError, new StepErrorLocation(this));

        var functionName = await FunctionName.Run(stateMonad, cancellationToken)
                .Map(x => x.GetStringAsync())
            ;

        if (functionName.IsFailure)
            return functionName.ConvertFailure<StringStream>();

        var scriptText = await ScriptText.Run(stateMonad, cancellationToken)
            .Map(x => x.GetStringAsync());

        if (scriptText.IsFailure)
            return scriptText.ConvertFailure<StringStream>();

        var parameters = await Parameters.Run(stateMonad, cancellationToken);

        if (parameters.IsFailure)
            return parameters.ConvertFailure<StringStream>();

        var nuixConnection = await stateMonad.GetOrCreateNuixConnection(this, false);

        if (nuixConnection.IsFailure)
            return nuixConnection.ConvertFailure<StringStream>()
                .MapError(x => x.WithLocation(this));

        var rubyFunctionParameters = parameters.Value
            .Select(x => new RubyFunctionParameter(ConvertString(x.Name), x.Name, true))
            .ToList();

        var parameterDict = parameters.Value
            .ToDictionary(
                x =>
                    new RubyFunctionParameter(ConvertString(x.Name), x.Name, true),
                x => x.Value
            )
            .Where(x => x.Value != null)
            .ToDictionary(x => x.Key, x => x.Value!);

        if (EntityStreamParameter != null)
        {
            var streamResult = await EntityStreamParameter.Run(stateMonad, cancellationToken);

            if (streamResult.IsFailure)
                return streamResult.ConvertFailure<StringStream>();

            const string streamParameter = "datastream";

            var parameter = new RubyFunctionParameter(
                ConvertString(streamParameter),
                streamParameter,
                true
            );

            rubyFunctionParameters.Add(parameter);

            parameterDict.Add(parameter, streamResult.Value);
        }

        var function = new RubyFunction<string>(
            ConvertString(functionName.Value),
            scriptText.Value,
            rubyFunctionParameters,
            new Version(5, 0),
            ArraySegment<NuixFeature>.Empty
        );

        var runResult = await nuixConnection.Value.RunFunctionAsync(
                stateMonad,
                this,
                function,
                parameterDict,
                CasePathParameter.IgnoresOpenCase.Instance,
                cancellationToken
            )
            .Map(x => new StringStream(x));

        if (runResult.IsFailure)
            return runResult
                .MapError(error => error.WithLocation(this))
                .ConvertFailure<StringStream>();

        return runResult.Value;
    }

    private static string ConvertString(string s)
    {
        if (string.IsNullOrWhiteSpace(s))
            return s;

        s = s.Replace(' ', '_');

        if (char.IsUpper(s.First()))
        {
            s = char.ToLowerInvariant(s[0]) + s[1..];
        }

        return s;
    }

    /// <summary>
    /// What to call this function.
    /// This will have spaces replaced with underscores and the first character will be made lowercase
    /// </summary>
    [StepProperty(1)]
    [RequiredVersion(RubyScriptStepBase<SCLNull>.NuixVersionKey, "8.2")]
    [Required]
    public IStep<StringStream> FunctionName { get; set; } = null!;

    /// <summary>
    /// The text of the script to run
    /// </summary>
    [StepProperty(2)]
    [Required]
    public IStep<StringStream> ScriptText { get; set; } = null!;

    /// <summary>
    /// Parameters to send to the script.
    /// You can access these by name (with spaces replaced by underscores and the first character lowercase).
    /// </summary>
    [StepProperty(3)]
    [Required]
    public IStep<Entity> Parameters { get; set; } = null!;

    /// <summary>
    /// Entities to stream to the script.
    /// The parameter name is 'entityStream'
    /// </summary>
    [StepProperty(4)]
    [DefaultValueExplanation("Do not stream entities")]
    public IStep<Array<Entity>>? EntityStreamParameter { get; set; } = null!;

    /// <inheritdoc />
    public override IStepFactory StepFactory => NuixRunScriptStepFactory.Instance;
}

/// <summary>
/// Run an arbitrary ruby script in nuix.
/// It should return a string.
/// </summary>
public class NuixRunScriptStepFactory : SimpleStepFactory<NuixRunScript, StringStream>
{
    private NuixRunScriptStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static SimpleStepFactory<NuixRunScript, StringStream> Instance { get; } =
        new NuixRunScriptStepFactory();
}
