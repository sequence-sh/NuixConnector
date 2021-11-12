using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using Reductech.EDR.ConnectorManagement.Base;
using Reductech.EDR.Core;
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

        var nuixSettings = SettingsHelpers.TryGetNuixSettings(stateMonad.Settings);

        if (nuixSettings.IsFailure)
            return nuixSettings.ConvertFailure<NuixConnection>();

        if (string.IsNullOrWhiteSpace(nuixSettings.Value.ExeConsolePath))
            return ErrorCode.MissingStepSettingsValue.ToErrorBuilder(
                nameof(ConnectorSettings.Settings),
                nameof(NuixSettings.ExeConsolePath)
            );

        var consoleArguments = TryGetConsoleArguments(nuixSettings.Value);

        if (consoleArguments.IsFailure)
            return consoleArguments.ConvertFailure<NuixConnection>();

        var scriptPath = GetScriptPath(nuixSettings.Value);

        var fileSystemHelper =
            stateMonad.ExternalContext.TryGetContext<IFileSystem>(ConnectorInjection.FileSystemKey);

        if (fileSystemHelper.IsFailure)
            return fileSystemHelper.ConvertFailure<NuixConnection>();

        if (!fileSystemHelper.Value.File.Exists(scriptPath))
            return ErrorCode.ExternalProcessNotFound.ToErrorBuilder(scriptPath);

        consoleArguments.Value.Add(scriptPath);

        var environmentVariables = TryGetEnvironmentVariables(nuixSettings.Value);

        if (environmentVariables.IsFailure)
            return environmentVariables.ConvertFailure<NuixConnection>();

        var r = stateMonad.ExternalContext.ExternalProcessRunner.StartExternalProcess(
            nuixSettings.Value.ExeConsolePath,
            consoleArguments.Value,
            environmentVariables.Value,
            Encoding.UTF8,
            stateMonad,
            callingStep
        );

        if (r.IsFailure)
            return r.ConvertFailure<NuixConnection>();

        var connection = new NuixConnection(
            r.Value,
            new NuixConnectionSettings(
                nuixSettings.Value.IgnoreWarningsRegex,
                nuixSettings.Value.IgnoreErrorsRegex
            )
        );

        var setResult = await stateMonad.SetVariableAsync(
            NuixVariableName,
            connection,
            true,
            callingStep,
            CancellationToken.None
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
    private static string GetScriptPath(NuixSettings settings)
    {
        if (settings.ScriptPath is not null)
            return settings.ScriptPath;

        var nuixAssembly = Assembly.GetAssembly(typeof(IRubyScriptStep))!;
        var scriptPath2  = Path.Combine(nuixAssembly.Location, "..", NuixGeneralScriptName);
        return scriptPath2;

        //var ev =
        //    sclSettings.Entity.TryGetValue(
        //        new EntityPropertyKey(
        //            new[] { SCLSettings.ConnectorsKey, NuixSettings.NuixSettingsKey, "path" }
        //        )
        //    ); //look for plugin directory

        //if (ev.HasValue)
        //{
        //    //use the plugin directory
        //    var pluginPath   = ev.Value.GetPrimitiveString();
        //    var absolutePath = PluginLoadContext.GetAbsolutePath(pluginPath);

        //    var directory = Path.GetDirectoryName(absolutePath);

        //    if (directory is not null)
        //    {
        //        var scriptPath = Path.Combine(directory, NuixGeneralScriptName);
        //        return scriptPath;
        //    }
        //}

        ////use the base directory
        //var scriptPath2 = Path.Combine(AppContext.BaseDirectory, NuixGeneralScriptName);
        //return scriptPath2;
    }

    private static Result<IReadOnlyDictionary<string, string>, IErrorBuilder>
        TryGetEnvironmentVariables(NuixSettings nuixSettings)
    {
        var dict = new Dictionary<string, string>();

        if (nuixSettings.EnvironmentVariables is not null)
        {
            foreach (var ep in nuixSettings.EnvironmentVariables)
                dict.Add(ep.Name, ep.Value.GetPrimitiveString());
        }

        if (nuixSettings.NuixUsername is not null)
            dict.Add("NUIX_USERNAME", nuixSettings.NuixUsername);

        if (nuixSettings.NuixPassword is not null)
            dict.Add("NUIX_PASSWORD", nuixSettings.NuixPassword);

        return dict;
    }

    /// <summary>
    /// Get the arguments that will be sent to the nuix console
    /// </summary>
    public static Result<List<string>, IErrorBuilder>
        TryGetConsoleArguments(NuixSettings nuixSettings)
    {
        var argumentsList = new List<string>();

        void MaybeAddBoolValue(string key, bool b)
        {
            if (b)
                argumentsList.Add("-" + key);
        }

        void MaybeAddStringKey(string key, string? value)
        {
            if (value is null)
                return;

            argumentsList.Add("-" + key);
            argumentsList.Add(value);
        }

        if (nuixSettings.ConsoleArguments is not null)
            argumentsList.AddRange(nuixSettings.ConsoleArguments);

        MaybeAddBoolValue("signout", nuixSettings.Signout);
        MaybeAddBoolValue("release", nuixSettings.Release);
        MaybeAddStringKey("licencesourcetype",     nuixSettings.LicenceSourceType);
        MaybeAddStringKey("licenceSourceLocation", nuixSettings.LicenseSourceLocation);
        MaybeAddStringKey("licenceType",           nuixSettings.LicenceType);
        MaybeAddStringKey("licenceWorkers",        nuixSettings.LicenceWorkers?.ToString());

        if (nuixSettings.ConsoleArgumentsPost is not null)
            argumentsList.AddRange(nuixSettings.ConsoleArgumentsPost);

        return argumentsList;
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
