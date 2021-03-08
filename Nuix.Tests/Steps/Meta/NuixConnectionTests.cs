using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using MELT;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Thinktecture;
using Thinktecture.Adapters;
using Thinktecture.IO;
using Xunit;
using Xunit.Sdk;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps.Meta
{

public static class NuixConnectionTestsHelper
{
    public static IStateMonad GetStateMonad(
        IExternalProcessRunner externalProcessRunner,
        ITestLoggerFactory testLoggerFactory,
        IFileSystem fileSystem) => GetStateMonad(
        testLoggerFactory,
        externalProcessRunner,
        fileSystem,
        new ConsoleAdapter()
    );

    public static IStateMonad GetStateMonad(
        ITestLoggerFactory testLoggerFactory,
        IExternalProcessRunner externalProcessRunner,
        IFileSystem fileSystem,
        IConsole console)
    {
        var nuixSettings = NuixSettings.CreateSettings(
            Constants.NuixConsoleExe,
            new Version(8, 0),
            true,
            Constants.AllNuixFeatures
        );

        var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(IRubyScriptStep));

        var monad = new StateMonad(
            testLoggerFactory.CreateLogger("Test"),
            nuixSettings,
            sfs,
            new ExternalContext(fileSystem, externalProcessRunner, console),
            new Dictionary<string, object>()
        );

        return monad;
    }

    public static IStateMonad GetStateMonadForProcess(ITestLoggerFactory testLoggerFactory) =>
        new StateMonad(
            testLoggerFactory.CreateLogger("NuixProcess"),
            new SCLSettings(Entity.Create()),
            null!,
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
            loggerFactory,
            FileSystemAdapter.Default
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
                null
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
}

public class NuixConnectionHelperTests
{
    [Fact]
    public async Task GetOrCreateNuixConnection_WhenConnectionExists_ReturnsConnection()
    {
        var loggerFactory = TestLoggerFactory.Create();
        var state         = NuixConnectionTestsHelper.GetStateMonadWithConnection(loggerFactory);

        var expected = state.GetVariable<NuixConnection>(NuixConnectionHelper.NuixVariableName);

        var createConnection = await state.GetOrCreateNuixConnection(null, false);

        Assert.True(createConnection.IsSuccess);
        Assert.Same(expected.Value, createConnection.Value);
    }

    [Fact]
    public async Task GetOrCreateNuixConnection_WhenReopenIsSet_DisposesOldConnection()
    {
        var loggerFactory = TestLoggerFactory.Create();
        var state         = NuixConnectionTestsHelper.GetStateMonadWithConnection(loggerFactory);

        var originalConnection =
            state.GetVariable<NuixConnection>(NuixConnectionHelper.NuixVariableName);

        var createConnection = await state.GetOrCreateNuixConnection(null, true);

        var processRef =
            originalConnection.Value.ExternalProcess as ExternalProcessMock.ProcessReferenceMock;

        Assert.True(createConnection.IsSuccess);
        Assert.True(processRef!.IsDisposed);
    }

    [Fact]
    public async Task GetOrCreateNuixConnection_WhenConnectionAlreadyDisposed_LogsMessage()
    {
        var loggerFactory = TestLoggerFactory.Create();
        var state         = NuixConnectionTestsHelper.GetStateMonadWithConnection(loggerFactory);

        var originalConnection =
            state.GetVariable<NuixConnection>(NuixConnectionHelper.NuixVariableName);

        originalConnection.Value.Dispose();

        var createConnection = await state.GetOrCreateNuixConnection(null, true);

        Assert.True(originalConnection.IsSuccess);
        Assert.True(createConnection.IsSuccess);

        loggerFactory.Sink.LogEntries
            .Should()
            .Contain(x => x.Message != null && x.Message.Equals("Connection already disposed."));
    }

    [Fact]
    public async Task GetOrCreateNuixConnection_OnStartExternalProcessFailure_ReturnsError()
    {
        var fakeExternalProcess =
            new ExternalProcessMock(1, NuixConnectionTestsHelper.GetCreateCaseAction())
            {
                ProcessPath = "WrongPath", ValidateArguments = false
            };

        var loggerFactory = TestLoggerFactory.Create();

        IStateMonad state =
            NuixConnectionTestsHelper.GetStateMonad(
                fakeExternalProcess,
                loggerFactory,
                FileSystemAdapter.Default
            );

        var createConnection = await state.GetOrCreateNuixConnection(null, true);

        Assert.True(createConnection.IsFailure);

        Assert.Equal(
            $"External Process Failed: 'Could not start '{Constants.NuixConsoleExe}''",
            createConnection.Error.AsString
        );
    }

    [Fact]
    public async Task CloseNuixConnectionAsync_WhenNoConnectionExists_DoesNothing()
    {
        var fakeExternalProcess = new ExternalProcessMock(
            1,
            NuixConnectionTestsHelper.GetCreateCaseAction()
        );

        IStateMonad state = NuixConnectionTestsHelper.GetStateMonad(
            fakeExternalProcess,
            TestLoggerFactory.Create(),
            FileSystemAdapter.Default
        );

        var ct = new CancellationToken();

        var actual = await state.CloseNuixConnectionAsync(null, ct);

        Assert.True(actual.IsSuccess);
        Assert.Equal(Unit.Default, actual);
    }

    [Fact]
    public async Task CloseNuixConnectionAsync_WhenConnectionExists_ClosesConnection()
    {
        var state =
            NuixConnectionTestsHelper.GetStateMonadWithConnection(TestLoggerFactory.Create());

        var ct = new CancellationToken();

        var actual     = await state.CloseNuixConnectionAsync(null, ct);
        var connection = state.GetVariable<NuixConnection>(NuixConnectionHelper.NuixVariableName);

        Assert.True(actual.IsSuccess);
        Assert.Equal(Unit.Default, actual);
        Assert.True(connection.IsFailure);
    }

    [Fact]
    public async Task CloseNuixConnectionAsync_ErrorOnClose_ReturnsError()
    {
        var state = NuixConnectionTestsHelper.GetStateMonadWithConnection(
            TestLoggerFactory.Create(),
            NuixConnectionTestsHelper.GetDoneAction()
        );

        var ct = new CancellationToken();

        var originalConnection =
            state.GetVariable<NuixConnection>(NuixConnectionHelper.NuixVariableName);

        Assert.True(originalConnection.IsSuccess);
        originalConnection.Value.Dispose();

        var actual = await state.CloseNuixConnectionAsync(null, ct);

        Assert.True(actual.IsFailure);

        Assert.Matches(
            $"Cannot access a disposed object\\.\\s+Object name: '{nameof(NuixConnection)}'",
            actual.Error.AsString
        );
    }

    [Fact]
    public async Task GetOrCreateNuixConnection_WhenScriptFileDoesNotExist_ReturnsError()
    {
        var fakeExternalProcess = new ExternalProcessMock(
            1,
            NuixConnectionTestsHelper.GetCreateCaseAction()
        );

        var fileMock =
            Mock.Of<IFile>(f => f.Exists(It.IsAny<string>()) == false);

        IStateMonad state = NuixConnectionTestsHelper.GetStateMonad(
            TestLoggerFactory.Create(),
            fakeExternalProcess,
            new FileSystemAdapter(Mock.Of<IDirectory>(), fileMock, Mock.Of<ICompression>()),
            new ConsoleAdapter()
        );

        var connection = await state.GetOrCreateNuixConnection(null, false);

        Assert.True(connection.IsFailure);

        Assert.Matches(
            $"Could not find.+{NuixConnectionHelper.NuixGeneralScriptName}'",
            connection.Error.AsString
        );
    }
}

public class NuixConnectionTests
{
    [Fact]
    public async Task SendDoneCommand_WritesDoneToExternalProcess()
    {
        var logFactory     = TestLoggerFactory.Create();
        var action         = NuixConnectionTestsHelper.GetDoneAction();
        var nuixConnection = NuixConnectionTestsHelper.GetNuixConnection(logFactory, action);

        var fakeExternalProcess = new ExternalProcessMock(
            1,
            NuixConnectionTestsHelper.GetCreateCaseAction()
        );

        var state = NuixConnectionTestsHelper.GetStateMonad(
            fakeExternalProcess,
            logFactory,
            FileSystemAdapter.Default
        );

        var ct = new CancellationToken();

        await nuixConnection.SendDoneCommand(state, null, ct);

        logFactory.Sink.LogEntries.Should()
            .Contain(x => x.Message != null && x.Message.Equals("Finished"));
    }

    [Fact]
    public async Task RunFunctionAsync_WhenDisposed_Throws()
    {
        var logFactory     = TestLoggerFactory.Create();
        var nuixConnection = NuixConnectionTestsHelper.GetNuixConnection(logFactory);

        var ct = new CancellationToken();

        nuixConnection.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => nuixConnection.RunFunctionAsync<Unit>(
                NuixConnectionTestsHelper.GetStateMonadForProcess(logFactory),
                null!,
                null!,
                new Dictionary<RubyFunctionParameter, object>(),
                CasePathParameter.IgnoresOpenCase.Instance,
                ct
            )
        );
    }

    [Fact]
    public async Task RunFunctionAsync_WithTwoEntityStreamParameters_ReturnsError()
    {
        var logFactory     = TestLoggerFactory.Create();
        var action         = NuixConnectionTestsHelper.GetDoneAction();
        var nuixConnection = NuixConnectionTestsHelper.GetNuixConnection(logFactory, action);
        var ct             = new CancellationToken();

        var stream1 = new List<Entity>().ToAsyncEnumerable().ToSequence();
        var stream2 = new List<Entity>().ToAsyncEnumerable().ToSequence();

        var dict = new Dictionary<RubyFunctionParameter, object>
        {
            { new RubyFunctionParameter("stream1Arg", "Stream1", false), stream1 },
            { new RubyFunctionParameter("stream2Arg", "Stream2", false), stream2 }
        };

        var stepParams = new ReadOnlyDictionary<RubyFunctionParameter, object>(dict);

        var step = new FakeNuixTwoStreamFunction();

        var result = await nuixConnection.RunFunctionAsync(
            NuixConnectionTestsHelper.GetStateMonadForProcess(logFactory),
            null,
            step.RubyScriptStepFactory.RubyFunction,
            stepParams,
            CasePathParameter.IgnoresOpenCase.Instance,
            ct
        );

        Assert.True(result.IsFailure);

        Assert.Equal(
            "A nuix function cannot have more than one Entity Array parameter.",
            result.Error.AsString
        );
    }

    [Fact]
    public async Task RunFunctionAsync_WhenFunctionHasEntityStream_ProcessStream()
    {
        var action = new ExternalProcessAction(
            new ConnectionCommand
            {
                Command            = "EntityStream",
                FunctionDefinition = "doesn't matter",
                Arguments          = new Dictionary<string, object>(),
                IsStream           = true
            }
        );

        var logFactory     = TestLoggerFactory.Create();
        var nuixConnection = NuixConnectionTestsHelper.GetNuixConnection(logFactory, action);
        var ct             = new CancellationToken();

        var entities = new List<Entity>
        {
            Entity.Create(("Property1", "Value1")), Entity.Create(("Property2", "Value2"))
        };

        var stream1 = entities.ToAsyncEnumerable().ToSequence();

        var dict = new Dictionary<RubyFunctionParameter, object>()
        {
            { new RubyFunctionParameter("entityStream", "EntityStream", false), stream1 }
        };

        var stepParams = new ReadOnlyDictionary<RubyFunctionParameter, object>(dict);

        var step = new FakeNuixStreamFunction();

        var result = await nuixConnection.RunFunctionAsync(
            NuixConnectionTestsHelper.GetStateMonadForProcess(logFactory),
            null,
            step.RubyScriptStepFactory.RubyFunction,
            stepParams,
            CasePathParameter.IgnoresOpenCase.Instance,
            ct
        );

        result.ShouldBeSuccessful(x => x.AsString);

        Assert.Equal(@"[{""Property1"":""Value1""},{""Property2"":""Value2""}]", result.Value);
    }

    private static async Task<(ITestLoggerFactory, Result<Unit, IErrorBuilder>)>
        GetActionResult(
            ConnectionOutput output,
            string[]? stdOut,
            string[]? stdErr)
    {
        const string? casePath = @"d:\case";
        const string? search   = "*.png";
        const string? tag      = "image";

        var helperAction = new ExternalProcessAction(
            new ConnectionCommand { Command = "Search", FunctionDefinition = "", IsHelper = true },
            new ConnectionOutput { Result = new ConnectionOutputResult { Data = "helper_success" } }
        ) { WriteToStdOut = stdOut, WriteToStdErr = stdErr };

        var action = new ExternalProcessAction(
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
        ) { WriteToStdOut = stdOut, WriteToStdErr = stdErr };

        var loggerFactory = TestLoggerFactory.Create();

        var nuixConnection =
            NuixConnectionTestsHelper.GetNuixConnection(loggerFactory, helperAction, action);

        var ct = new CancellationToken();

        var dict = new Dictionary<RubyFunctionParameter, object>()
        {
            {
                new RubyFunctionParameter("pathArg", nameof(NuixSearchAndTag.CasePath), false),
                casePath
            },
            {
                new RubyFunctionParameter("searchArg", nameof(NuixSearchAndTag.SearchTerm), false),
                search
            },
            { new RubyFunctionParameter("tagArg", nameof(NuixSearchAndTag.Tag), false), tag }
        };

        var stepParams = new ReadOnlyDictionary<RubyFunctionParameter, object>(dict);

        var step = new NuixSearchAndTag();

        var result = await nuixConnection.RunFunctionAsync(
            NuixConnectionTestsHelper.GetStateMonadForProcess(loggerFactory),
            null,
            step.RubyScriptStepFactory.RubyFunction,
            stepParams,
            CasePathParameter.IgnoresOpenCase.Instance,
            ct
        );

        return (loggerFactory, result);
    }

    [Fact]
    public async Task RunFunctionAsync_WhenConnectionOutputPropertiesAreNull_ReturnsError()
    {
        var output = new ConnectionOutput { Result = null };

        var (_, result) = await GetActionResult(output, null, null);

        result.ShouldBeFailure();

        Assert.Equal(
            $"External Process Failed: '{nameof(ConnectionOutput)} must have at least one property set'",
            result.Error.AsString
        );
    }

    [Fact]
    public async Task RunFunctionAsync_WhenConnectionOutputIsNotValid_ReturnsError()
    {
        var output = new ConnectionOutput
        {
            Log = new ConnectionOutputLog(), Result = new ConnectionOutputResult()
        };

        var (_, result) = await GetActionResult(output, null, null);

        Assert.True(result.IsFailure);

        Assert.Equal(
            $"External Process Failed: '{nameof(ConnectionOutput)} can only have one property set'",
            result.Error.AsString
        );
    }

    [Theory]
    [InlineData("(eval):9: warning:", "This is a warning.")]
    [InlineData("ERROR ",             "This is an error.")]
    public async Task RunFunctionAsync_WhenJavaWritesToStdErr_LogsWarning(
        string prefix,
        string message)
    {
        var output = new ConnectionOutput { Result = new ConnectionOutputResult() };

        var (loggerFactory, result) = await GetActionResult(
            output,
            null,
            new[] { $"{prefix}{message}" }
        );

        Assert.True(result.IsSuccess);

        loggerFactory.Sink.LogEntries
            .Should()
            .Contain(
                x => x.LogLevel == LogLevel.Warning
                  && x.Message != null && x.Message.Equals(message)
            );
    }

    [Fact]
    public async Task RunFunctionAsync_WhenItCantParseJson_ReturnsFailure()
    {
        const string? message = "A random message";

        var output = new ConnectionOutput { Result = new ConnectionOutputResult() };

        var (_, result) = await GetActionResult(
            output,
            new[] { message },
            null
        );

        Assert.True(result.IsFailure);

        Assert.Equal(
            ErrorCode.CouldNotParse.GetFormattedMessage(message, nameof(ConnectionOutput)),
            result.Error.AsString
        );
    }
}

}
