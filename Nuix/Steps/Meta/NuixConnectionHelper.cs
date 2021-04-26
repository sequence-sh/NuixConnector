using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Connectors;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta
{

/// <summary>
/// Helps with connections to nuix.
/// </summary>
public static class NuixConnectionHelper
{
    /// <summary>
    /// The name of the Nuix Connector Script
    /// </summary>
    internal const string NuixGeneralScriptName = "nuixconnectorscript.rb";

    /// <summary>
    /// The name of the Nuix Variable in the SCL state
    /// </summary>
    internal static readonly VariableName NuixVariableName =
        new("ReductechNuixConnection");

    /// <summary>
    /// Gets or creates a connection to nuix.
    /// </summary>
    public static async Task<Result<NuixConnection, IErrorBuilder>> GetOrCreateNuixConnection(
        this IStateMonad stateMonad,
        IStep? callingStep,
        bool reopen)
    {
        var currentConnection = stateMonad.GetVariable<NuixConnection>(NuixVariableName);

        if (currentConnection.IsSuccess)
        {
            // TODO: What happens if the connection is closed/disposed?
            if (reopen)
            {
                if (currentConnection.Value.IsDisposed)
                {
                    stateMonad.Logger.LogDebug("Connection already disposed.");
                }
                else
                {
                    currentConnection.Value.ExternalProcess.WaitForExit(1000);

                    currentConnection.Value
                        .Dispose(); //Get rid of this connection and open a new one
                }
            }

            else
                return currentConnection.Value;
        }

        var settings = stateMonad.Settings;

        var connectionSettings = NuixConnectionSettings.Create(settings);

        var consoleArguments = TryGetConsoleArguments(settings);

        if (consoleArguments.IsFailure)
            return consoleArguments.ConvertFailure<NuixConnection>();

        var scriptPath = GetScriptPath(settings);

        var fileSystemHelper =
            stateMonad.ExternalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

        if (fileSystemHelper.IsFailure)
            return fileSystemHelper.ConvertFailure<NuixConnection>();

        if (!fileSystemHelper.Value.File.Exists(scriptPath))
            return ErrorCode.ExternalProcessNotFound.ToErrorBuilder(scriptPath);

        consoleArguments.Value.arguments.Add(scriptPath);

        var environmentVariables = TryGetEnvironmentVariables(settings);

        if (environmentVariables.IsFailure)
            return environmentVariables.ConvertFailure<NuixConnection>();

        var r = stateMonad.ExternalContext.ExternalProcessRunner.StartExternalProcess(
            consoleArguments.Value.consolePath,
            consoleArguments.Value.arguments,
            environmentVariables.Value,
            Encoding.UTF8,
            stateMonad,
            callingStep
        );

        if (r.IsFailure)
            return r.ConvertFailure<NuixConnection>();

        var connection = new NuixConnection(r.Value, connectionSettings);

        var setResult = await stateMonad.SetVariableAsync(
            NuixVariableName,
            connection,
            true,
            callingStep
        );

        if (setResult.IsFailure)
            return setResult.ConvertFailure<NuixConnection>()
                .MapError(x => x.ToErrorBuilder);

        return connection;
    }

    /// <summary>
    /// Gets the nuix script path - first looking in the plugin path folder, then in the base directory
    /// </summary>
    /// <returns></returns>
    private static string GetScriptPath(SCLSettings sclSettings)
    {
        var ev =
            sclSettings.Entity.TryGetValue(
                new EntityPropertyKey(
                    new[] { SCLSettings.ConnectorsKey, NuixSettings.NuixSettingsKey, "path" }
                )
            ); //look for plugin directory

        if (ev.HasValue)
        {
            //use the plugin directory
            var pluginPath   = ev.Value.GetPrimitiveString();
            var absolutePath = PluginLoadContext.GetAbsolutePath(pluginPath);

            var directory = Path.GetDirectoryName(absolutePath);

            if (directory is not null)
            {
                var scriptPath = Path.Combine(directory, NuixGeneralScriptName);
                return scriptPath;
            }
        }

        //use the 
        var scriptPath2 = Path.Combine(AppContext.BaseDirectory, NuixGeneralScriptName);
        return scriptPath2;
    }

    private static Result<IReadOnlyDictionary<string, string>, IErrorBuilder>
        TryGetEnvironmentVariables(SCLSettings sclSettings)
    {
        var ev =
            sclSettings.Entity.TryGetValue(
                new EntityPropertyKey(
                    new[]
                    {
                        SCLSettings.ConnectorsKey, NuixSettings.NuixSettingsKey,
                        NuixSettings.EnvironmentVariablesKey
                    }
                )
            );

        if (ev.HasNoValue)
            return new Dictionary<string, string>();

        if (ev.Value is not EntityValue.NestedEntity entity)
            return ErrorCode.MissingStepSettingsValue.ToErrorBuilder(
                NuixSettings.NuixSettingsKey,
                NuixSettings.EnvironmentVariablesKey
            );

        var dict = new Dictionary<string, string>();

        foreach (var ep in entity.Value.Dictionary.Values)
            dict.Add(ep.Name, ep.BestValue.GetPrimitiveString());

        var username = sclSettings.Entity.TryGetNestedString(
            SCLSettings.ConnectorsKey,
            NuixSettings.NuixSettingsKey,
            NuixSettings.NuixUsernameKey
        );

        if (username.HasValue)
            dict.Add(NuixSettings.NuixUsernameKey, username.Value);

        var password = sclSettings.Entity.TryGetNestedString(
            SCLSettings.ConnectorsKey,
            NuixSettings.NuixSettingsKey,
            NuixSettings.NuixPasswordKey
        );

        if (password.HasValue)
            dict.Add(NuixSettings.NuixPasswordKey, password.Value);

        return dict;
    }

    /// <summary>
    /// Get the arguments that will be sent to the nuix console
    /// </summary>
    public static Result<(string consolePath, List<string> arguments), IErrorBuilder>
        TryGetConsoleArguments(SCLSettings sclSettings)
    {
        var pathResult = sclSettings.Entity.TryGetNestedString(
            SCLSettings.ConnectorsKey,
            NuixSettings.NuixSettingsKey,
            NuixSettings.ConsolePathKey
        );

        if (pathResult.HasNoValue)
            return ErrorCode.MissingStepSettingsValue.ToErrorBuilder(
                NuixSettings.NuixSettingsKey,
                NuixSettings.ConsolePathKey
            );

        var argumentsList = new List<string>();

        void MaybeAddBoolValue(string key)
        {
            var b = sclSettings.Entity.TryGetNestedBool(
                SCLSettings.ConnectorsKey,
                NuixSettings.NuixSettingsKey,
                key
            );

            if (b)
                argumentsList.Add("-" + key);
        }

        void MaybeAddStringKey(string key)
        {
            var s = sclSettings.Entity.TryGetNestedString(
                SCLSettings.ConnectorsKey,
                NuixSettings.NuixSettingsKey,
                key
            );

            if (s.HasValue)
            {
                argumentsList.Add("-" + key);
                argumentsList.Add(s.Value);
            }
        }

        var extraConsoleArguments =
            sclSettings.Entity.TryGetNestedList(
                SCLSettings.ConnectorsKey,
                NuixSettings.NuixSettingsKey,
                NuixSettings.ConsoleArgumentsKey
            );

        if (extraConsoleArguments.HasValue)
            argumentsList.AddRange(extraConsoleArguments.Value);

        MaybeAddBoolValue(NuixSettings.SignoutKey);
        MaybeAddBoolValue(NuixSettings.ReleaseKey);
        MaybeAddStringKey(NuixSettings.LicenceSourceTypeKey);
        MaybeAddStringKey(NuixSettings.LicenceSourceLocationKey);
        MaybeAddStringKey(NuixSettings.LicenceTypeKey);
        MaybeAddStringKey(NuixSettings.LicenceWorkersKey);

        var postConsoleArguments =
            sclSettings.Entity.TryGetNestedList(
                SCLSettings.ConnectorsKey,
                NuixSettings.NuixSettingsKey,
                NuixSettings.ConsoleArgumentsPostKey
            );

        if (postConsoleArguments.HasValue)
            argumentsList.AddRange(postConsoleArguments.Value);

        return (pathResult.Value, argumentsList);
    }

    /// <summary>
    /// Close the nuix connection if it is open.
    /// </summary>
    public static async Task<Result<Unit, IErrorBuilder>> CloseNuixConnectionAsync(
        this IStateMonad stateMonad,
        IStep? callingStep,
        CancellationToken cancellationToken)
    {
        var currentConnection = stateMonad.GetVariable<NuixConnection>(NuixVariableName);

        if (currentConnection.IsFailure)
            return Unit.Default; //nothing to do

        try
        {
            await currentConnection.Value.SendDoneCommand(stateMonad, null, cancellationToken);

            currentConnection.Value.ExternalProcess.WaitForExit(1000);
            currentConnection.Value.Dispose();
        }
        catch (Exception e)
        {
            return new ErrorBuilder(e, ErrorCode.ExternalProcessError);
        }

        await stateMonad.RemoveVariableAsync(NuixVariableName, true, callingStep);

        return Unit.Default;
    }
}

}
