using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MELT;
using Moq;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;
using Reductech.Utilities.Testing;
using Xunit;
using Xunit.Sdk;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps.Meta
{

public static class NuixConnectionTestsHelper
{
    public static IStateMonad GetStateMonad(
        IExternalProcessRunner externalProcessRunner,
        ITestLoggerFactory testLoggerFactory) => GetStateMonad(
        testLoggerFactory,
        externalProcessRunner,
        FileSystemHelper.Instance
    );

    public static IStateMonad GetStateMonad(
        ITestLoggerFactory testLoggerFactory,
        IExternalProcessRunner externalProcessRunner,
        IFileSystemHelper fileSystemHelper)
    {
        var nuixSettings = new NuixSettings(
            true,
            Constants.NuixSettingsList.First().NuixExeConsolePath,
            new Version(8, 8),
            Constants.AllNuixFeatures
        );

        var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(IRubyScriptStep));

        var monad = new StateMonad(
            testLoggerFactory.CreateLogger("Test"),
            nuixSettings,
            externalProcessRunner,
            fileSystemHelper,
            sfs
        );

        return monad;
    }

    public static NuixConnection GetNuixConnection(
        ExternalProcessAction action,
        int expectedTimesStarted = 1)
    {
        var fakeExternalProcess = new ExternalProcessMock(expectedTimesStarted, action)
        {
            ProcessPath = Constants.NuixSettingsList.First().NuixExeConsolePath
        };

        var process = fakeExternalProcess.StartExternalProcess(
            fakeExternalProcess.ProcessPath,
            fakeExternalProcess.ProcessArgs,
            fakeExternalProcess.ProcessEncoding
        );

        if (process.IsFailure)
            throw new XunitException("Failed to create a mock Nuix process");

        var connection = new NuixConnection(process.Value);

        return connection;
    }

    public static IStateMonad GetStateMonadWithConnection(ITestLoggerFactory loggerFactory) =>
        GetStateMonadWithConnection(loggerFactory, GetCreateCaseAction());

    public static IStateMonad GetStateMonadWithConnection(
        ITestLoggerFactory loggerFactory,
        params ExternalProcessAction[] actions)
    {
        var fakeExternalProcess = new ExternalProcessMock(2, actions);

        IStateMonad state = GetStateMonad(fakeExternalProcess, loggerFactory);

        var nuixSettings = state.GetSettings<INuixSettings>();

        fakeExternalProcess.ProcessPath = nuixSettings.Value.NuixExeConsolePath;

        var process = state.ExternalProcessRunner.StartExternalProcess(
            fakeExternalProcess.ProcessPath,
            fakeExternalProcess.ProcessArgs,
            fakeExternalProcess.ProcessEncoding
        );

        if (process.IsFailure)
            throw new XunitException("Failed to create a mock Nuix process");

        var connection = new NuixConnection(process.Value);

        var setResult = state.SetVariable(NuixConnectionHelper.NuixVariableName, connection);

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
    public void GetOrCreateNuixConnection_WhenConnectionExists_ReturnsConnection()
    {
        var loggerFactory = TestLoggerFactory.Create();
        var state         = NuixConnectionTestsHelper.GetStateMonadWithConnection(loggerFactory);

        var expected = state.GetVariable<NuixConnection>(NuixConnectionHelper.NuixVariableName);

        var createConnection = state.GetOrCreateNuixConnection(false);

        Assert.True(createConnection.IsSuccess);
        Assert.Same(expected.Value, createConnection.Value);
    }

    [Fact]
    public void GetOrCreateNuixConnection_WhenReopenIsSet_DisposesOldConnection()
    {
        var loggerFactory = TestLoggerFactory.Create();
        var state         = NuixConnectionTestsHelper.GetStateMonadWithConnection(loggerFactory);

        var originalConnection =
            state.GetVariable<NuixConnection>(NuixConnectionHelper.NuixVariableName);

        var createConnection = state.GetOrCreateNuixConnection(true);

        var processRef =
            originalConnection.Value.ExternalProcess as ExternalProcessMock.ProcessReferenceMock;

        Assert.True(createConnection.IsSuccess);
        Assert.True(processRef!.IsDisposed);
    }

    [Fact]
    public void GetOrCreateNuixConnection_WhenConnectionAlreadyDisposed_LogsMessage()
    {
        var loggerFactory = TestLoggerFactory.Create();
        var state         = NuixConnectionTestsHelper.GetStateMonadWithConnection(loggerFactory);

        var originalConnection =
            state.GetVariable<NuixConnection>(NuixConnectionHelper.NuixVariableName);

        originalConnection.Value.Dispose();

        var createConnection = state.GetOrCreateNuixConnection(true);

        Assert.True(originalConnection.IsSuccess);
        Assert.True(createConnection.IsSuccess);

        loggerFactory.Sink.LogEntries
            .Should()
            .Contain(x => x.Message != null && x.Message.Equals("Connection already disposed."));
    }

    [Fact]
    public void GetOrCreateNuixConnection_OnStartExternalProcessFailure_ReturnsError()
    {
        var fakeExternalProcess =
            new ExternalProcessMock(1, NuixConnectionTestsHelper.GetCreateCaseAction())
            {
                ProcessPath = "WrongPath", ValidateArguments = false
            };

        var loggerFactory = TestLoggerFactory.Create();

        IStateMonad state =
            NuixConnectionTestsHelper.GetStateMonad(fakeExternalProcess, loggerFactory);

        var createConnection = state.GetOrCreateNuixConnection(true);

        Assert.True(createConnection.IsFailure);

        Assert.Equal(
            $"External Process Failed: 'Could not start '{Constants.NuixSettingsList.First().NuixExeConsolePath}''",
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
            TestLoggerFactory.Create()
        );

        var ct = new CancellationToken();

        var actual = await state.CloseNuixConnectionAsync(ct);

        Assert.True(actual.IsSuccess);
        Assert.Equal(Unit.Default, actual);
    }

    [Fact]
    public async Task CloseNuixConnectionAsync_WhenConnectionExists_ClosesConnection()
    {
        var state =
            NuixConnectionTestsHelper.GetStateMonadWithConnection(TestLoggerFactory.Create());

        var ct = new CancellationToken();

        var actual     = await state.CloseNuixConnectionAsync(ct);
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

        var actual = await state.CloseNuixConnectionAsync(ct);

        Assert.True(actual.IsFailure);

        Assert.Matches(
            $"Cannot access a disposed object\\.\\s+Object name: '{nameof(NuixConnection)}'",
            actual.Error.AsString
        );
    }

    [Fact]
    public void GetOrCreateNuixConnection_WhenScriptFileDoesNotExist_ReturnsError()
    {
        var fakeExternalProcess = new ExternalProcessMock(
            1,
            NuixConnectionTestsHelper.GetCreateCaseAction()
        );

        var fakeFileSystemHelper =
            Mock.Of<IFileSystemHelper>(f => f.DoesFileExist(It.IsAny<string>()) == false);

        IStateMonad state = NuixConnectionTestsHelper.GetStateMonad(
            TestLoggerFactory.Create(),
            fakeExternalProcess,
            fakeFileSystemHelper
        );

        var connection = state.GetOrCreateNuixConnection(false);

        Assert.True(connection.IsFailure);
        Assert.Matches("Could not find.+edr-nuix-connector.rb'", connection.Error.AsString);
    }
}

public class NuixConnectionTests
{
    [Fact]
    public async Task SendDoneCommand_WritesDoneToExternalProcess()
    {
        var action         = NuixConnectionTestsHelper.GetDoneAction();
        var nuixConnection = NuixConnectionTestsHelper.GetNuixConnection(action);

        var fakeExternalProcess = new ExternalProcessMock(
            1,
            NuixConnectionTestsHelper.GetCreateCaseAction()
        );

        var logFactory = TestLoggerFactory.Create();

        var state = NuixConnectionTestsHelper.GetStateMonad(fakeExternalProcess, logFactory);
        var ct    = new CancellationToken();

        await nuixConnection.SendDoneCommand(state, ct);

        logFactory.Sink.LogEntries.Should()
            .Contain(x => x.Message != null && x.Message.Equals("Finished"));
    }

    [Fact]
    public async Task RunFunctionAsync_WhenDisposed_Throws()
    {
        var nuixConnection = NuixConnectionTestsHelper.GetNuixConnection(null!);

        var ct = new CancellationToken();

        nuixConnection.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () =>
                nuixConnection.RunFunctionAsync<Unit>(
                    TestLoggerFactory.Create().CreateLogger("Test"),
                    null!,
                    null!,
                    CasePathParameter.NoCasePath.Instance,
                    ct
                )
        );
    }

    [Fact]
    public async Task RunFunctionAsync_WithTwoEntityStreamParameters_ReturnsError()
    {
        var action         = NuixConnectionTestsHelper.GetDoneAction();
        var nuixConnection = NuixConnectionTestsHelper.GetNuixConnection(action);
        var logger         = TestLoggerFactory.Create().CreateLogger("Test");
        var ct             = new CancellationToken();

        var stream1 = new List<Entity>().ToAsyncEnumerable().ToSequence();
        var stream2 = new List<Entity>().ToAsyncEnumerable().ToSequence();

        var dict = new Dictionary<RubyFunctionParameter, object>
        {
            { new RubyFunctionParameter("stream1Arg", "Stream1", false, null), stream1 },
            { new RubyFunctionParameter("stream2Arg", "Stream2", false, null), stream2 }
        };

        var stepParams = new ReadOnlyDictionary<RubyFunctionParameter, object>(dict);

        var step = new FakeNuixTwoStreamFunction();

        var result = await nuixConnection.RunFunctionAsync(
            logger,
            step.RubyScriptStepFactory.RubyFunction,
            stepParams,
            CasePathParameter.NoCasePath.Instance,
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

        var nuixConnection = NuixConnectionTestsHelper.GetNuixConnection(action);
        var logger         = TestLoggerFactory.Create().CreateLogger("Test");
        var ct             = new CancellationToken();

        var entities = new List<Entity>
        {
            Entity.Create(("Property1", "Value1")), Entity.Create(("Property2", "Value2"))
        };

        var stream1 = entities.ToAsyncEnumerable().ToSequence();

        var dict = new Dictionary<RubyFunctionParameter, object>()
        {
            { new RubyFunctionParameter("entityStream", "EntityStream", false, null), stream1 }
        };

        var stepParams = new ReadOnlyDictionary<RubyFunctionParameter, object>(dict);

        var step = new FakeNuixStreamFunction();

        var result = await nuixConnection.RunFunctionAsync(
            logger,
            step.RubyScriptStepFactory.RubyFunction,
            stepParams,
            CasePathParameter.NoCasePath.Instance,
            ct
        );

        result.ShouldBeSuccessful(x => x.AsString);

        Assert.Equal(@"[{""Property1"":""Value1""},{""Property2"":""Value2""}]", result.Value);
    }

    [Fact]
    public async Task RunFunctionAsync_WhenConnectionOutputPropertiesAreNull_ReturnsError()
    {
        var casePath = @"d:\case";

        var action = new ExternalProcessAction(
            new ConnectionCommand
            {
                Command            = "MigrateCase",
                FunctionDefinition = "",
                Arguments = new Dictionary<string, object>
                {
                    { nameof(NuixMigrateCase.CasePath), casePath }
                }
            },
            new ConnectionOutput { Result = null }
        );

        var nuixConnection = NuixConnectionTestsHelper.GetNuixConnection(action);
        var logger         = TestLoggerFactory.Create().CreateLogger("Test");
        var ct             = new CancellationToken();

        var dict = new Dictionary<RubyFunctionParameter, object>()
        {
            {
                new RubyFunctionParameter("pathArg", nameof(NuixMigrateCase.CasePath), false, null),
                casePath
            }
        };

        var stepParams = new ReadOnlyDictionary<RubyFunctionParameter, object>(dict);

        var step = new NuixMigrateCase();

        var result = await nuixConnection.RunFunctionAsync(
            logger,
            step.RubyScriptStepFactory.RubyFunction,
            stepParams,
            CasePathParameter.NoCasePath.Instance,
            ct
        );

        result.ShouldBeFailure();

        Assert.Equal(
            $"External Process Failed: '{nameof(ConnectionOutput)} must have at least one property set'",
            result.Error.AsString
        );
    }

    [Fact]
    public async Task RunFunctionAsync_WhenConnectionOutputIsNotValid_ReturnsError()
    {
        var casePath = @"d:\case";

        var action = new ExternalProcessAction(
            new ConnectionCommand
            {
                Command            = "MigrateCase",
                FunctionDefinition = "",
                Arguments = new Dictionary<string, object>
                {
                    { nameof(NuixMigrateCase.CasePath), casePath }
                }
            },
            new ConnectionOutput
            {
                Log = new ConnectionOutputLog(), Result = new ConnectionOutputResult()
            }
        );

        var nuixConnection = NuixConnectionTestsHelper.GetNuixConnection(action);
        var logger         = TestLoggerFactory.Create().CreateLogger("Test");
        var ct             = new CancellationToken();

        var dict = new Dictionary<RubyFunctionParameter, object>()
        {
            {
                new RubyFunctionParameter("pathArg", nameof(NuixMigrateCase.CasePath), false, null),
                casePath
            }
        };

        var stepParams = new ReadOnlyDictionary<RubyFunctionParameter, object>(dict);

        var step = new NuixMigrateCase();

        var result = await nuixConnection.RunFunctionAsync(
            logger,
            step.RubyScriptStepFactory.RubyFunction,
            stepParams,
            CasePathParameter.NoCasePath.Instance,
            ct
        );

        Assert.True(result.IsFailure);

        Assert.Equal(
            $"External Process Failed: '{nameof(ConnectionOutput)} can only have one property set'",
            result.Error.AsString
        );
    }
}

}
