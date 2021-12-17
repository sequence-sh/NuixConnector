using System.Collections.ObjectModel;
using System.IO.Abstractions.TestingHelpers;
using System.Threading;
using MELT;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.ExternalProcesses;
using Xunit.Sdk;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps.Meta;

public static class ConnectionTestsHelper
{
    public static IStateMonad GetStateMonad(
        IExternalProcessRunner externalProcessRunner,
        ITestLoggerFactory testLoggerFactory) => GetStateMonad(
        testLoggerFactory,
        externalProcessRunner,
        ConsoleAdapter.Instance
    );

    public static IStateMonad GetStateMonad(
        ITestLoggerFactory testLoggerFactory,
        IExternalProcessRunner externalProcessRunner,
        IConsole console)
    {
        var sfs = SettingsHelpers.CreateStepFactoryStore(
            new NuixSettings(
                Constants.NuixConsoleExe,
                new Version(8, 0),
                true,
                Constants.AllNuixFeatures
            )
        );

        var fileSystem = new MockFileSystem(
            new Dictionary<string, MockFileData>()
            {
                { NuixConnectionHelper.NuixGeneralScriptName, "Ultimate Nuix Script" }
            },
            AppContext.BaseDirectory
        );

        var externalContext = new ExternalContext(
            externalProcessRunner,
            null!,
            console,
            (ConnectorInjection.FileSystemKey, fileSystem)
        );

        var monad = new StateMonad(
            testLoggerFactory.CreateLogger("Test"),
            sfs,
            externalContext,
            new Dictionary<string, object>()
        );

        return monad;
    }

    public static IStateMonad GetStateMonadForProcess(ITestLoggerFactory testLoggerFactory) =>
        new StateMonad(
            testLoggerFactory.CreateLogger("NuixProcess"),
            StepFactoryStore.TryCreateFromAssemblies(
                    null!,
                    typeof(IStep).Assembly,
                    typeof(IRubyScriptStep).Assembly
                )
                .Value,
            null!,
            new Dictionary<string, object>()
        );

    public static NuixConnection GetNuixConnection(
        ITestLoggerFactory loggerFactory,
        params ExternalProcessAction[] actions) => GetNuixConnection(loggerFactory, 1, actions);

    public static NuixConnection GetNuixConnection(
        ITestLoggerFactory loggerFactory,
        int expectedTimesStarted,
        params ExternalProcessAction[] actions)
    {
        var fakeExternalProcess = new ExternalProcessMock(expectedTimesStarted, actions)
        {
            ProcessPath = Constants.NuixConsoleExe
        };

        var process = fakeExternalProcess.StartExternalProcess(
            fakeExternalProcess.ProcessPath,
            fakeExternalProcess.ProcessArgs,
            new Dictionary<string, string>(),
            fakeExternalProcess.ProcessEncoding,
            GetStateMonadForProcess(loggerFactory),
            null
        );

        if (process.IsFailure)
            throw new XunitException("Failed to create a mock Nuix process");

        var connection = new NuixConnection(process.Value, NuixConnectionSettings.Default);

        return connection;
    }

    public static IStateMonad GetStateMonadWithConnection(ITestLoggerFactory loggerFactory) =>
        GetStateMonadWithConnection(loggerFactory, GetCreateCaseAction());

    public static IStateMonad GetStateMonadWithConnection(
        ITestLoggerFactory loggerFactory,
        params ExternalProcessAction[] actions)
    {
        var fakeExternalProcess = new ExternalProcessMock(2, actions);

        IStateMonad state = GetStateMonad(
            fakeExternalProcess,
            loggerFactory
        );

        fakeExternalProcess.ProcessPath = Constants.NuixConsoleExe;

        var process = state.ExternalContext.ExternalProcessRunner.StartExternalProcess(
            fakeExternalProcess.ProcessPath,
            fakeExternalProcess.ProcessArgs,
            new Dictionary<string, string>(),
            fakeExternalProcess.ProcessEncoding,
            GetStateMonadForProcess(loggerFactory),
            null
        );

        if (process.IsFailure)
            throw new XunitException("Failed to create a mock Nuix process");

        var connection = new NuixConnection(process.Value, NuixConnectionSettings.Default);

        var setResult = state.SetVariableAsync(
                NuixConnectionHelper.NuixVariableName,
                connection,
                true,
                null,
                CancellationToken.None
            )
            .Result;

        if (setResult.IsFailure)
            throw new XunitException("Could not set existing connection on state monad.");

        return state;
    }

    public static ExternalProcessAction GetCreateCaseAction()
    {
        return new(
            new ConnectionCommand
            {
                Command            = "CreateCase",
                FunctionDefinition = "",
                Arguments = new Dictionary<string, object>
                {
                    { nameof(NuixCreateCase.CasePath), "d:\\case" },
                    { nameof(NuixCreateCase.CaseName), "Integration Test Case" },
                    { nameof(NuixCreateCase.Investigator), "Mark" }
                }
            },
            new ConnectionOutput { Result = new ConnectionOutputResult { Data = null } }
        );
    }

    public static ExternalProcessAction GetDoneAction()
    {
        return new(
            new ConnectionCommand { Command = "done" },
            new ConnectionOutput
            {
                Log = new ConnectionOutputLog
                {
                    Message    = "Finished",
                    Severity   = "info",
                    Time       = DateTime.Now.ToString("G17"),
                    StackTrace = ""
                }
            }
        );
    }

    public static readonly ExternalProcessAction SearchHelperAction = new(
        new ConnectionCommand { Command = "Search", FunctionDefinition = "", IsHelper = true },
        new ConnectionOutput { Result   = new ConnectionOutputResult { Data = "helper_success" } }
    );

    public static readonly ExternalProcessAction ExpandHelperAction = new(
        new ConnectionCommand
        {
            Command = "ExpandSearch", FunctionDefinition = "", IsHelper = true
        },
        new ConnectionOutput { Result = new ConnectionOutputResult { Data = "helper_success" } }
    );

    public static ExternalProcessAction
        SearchAndTagAction(string casePath, string search, string tag, ConnectionOutput output) =>
        new(
            new ConnectionCommand
            {
                Command            = "SearchAndTag",
                FunctionDefinition = "",
                Arguments = new Dictionary<string, object>
                {
                    { nameof(NuixSearchAndTag.CasePath), casePath },
                    { nameof(NuixSearchAndTag.SearchTerm), search },
                    { nameof(NuixSearchAndTag.Tag), tag }
                }
            },
            output
        );

    public static ReadOnlyDictionary<RubyFunctionParameter, ISCLObject>
        SearchAndTagParams(string casePath, string search, string tag) => new(
        new Dictionary<RubyFunctionParameter, ISCLObject>
        {
            {
                new RubyFunctionParameter("pathArg", nameof(NuixSearchAndTag.CasePath), false),
                new StringStream(casePath)
            },
            {
                new RubyFunctionParameter("searchArg", nameof(NuixSearchAndTag.SearchTerm), false),
                new StringStream(search)
            },
            { new RubyFunctionParameter("tagArg", nameof(NuixSearchAndTag.Tag), false), new StringStream(tag) }
        }
    );
}
