using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MELT;
using Moq;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Util;
using Thinktecture.Adapters;
using Thinktecture.IO;
using Xunit;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps.Meta
{

using Reductech.EDR.Core;

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

}
