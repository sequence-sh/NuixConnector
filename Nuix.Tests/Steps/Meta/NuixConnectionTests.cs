using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using FluentAssertions;
using MELT;
using Microsoft.Extensions.Logging;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.TestHarness;
using Xunit;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps.Meta;

public class NuixConnectionTests
{
    [Fact]
    public async Task SendDoneCommand_WritesDoneToExternalProcess()
    {
        var logFactory     = TestLoggerFactory.Create();
        var action         = ConnectionTestsHelper.GetDoneAction();
        var nuixConnection = ConnectionTestsHelper.GetNuixConnection(logFactory, action);

        var fakeExternalProcess = new ExternalProcessMock(
            1,
            ConnectionTestsHelper.GetCreateCaseAction()
        );

        var state = ConnectionTestsHelper.GetStateMonad(
            fakeExternalProcess,
            logFactory
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
        var nuixConnection = ConnectionTestsHelper.GetNuixConnection(logFactory);

        var ct = new CancellationToken();

        nuixConnection.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => nuixConnection.RunFunctionAsync<Unit>(
                ConnectionTestsHelper.GetStateMonadForProcess(logFactory),
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
        var action         = ConnectionTestsHelper.GetDoneAction();
        var nuixConnection = ConnectionTestsHelper.GetNuixConnection(logFactory, action);
        var ct             = new CancellationToken();

        var stream1 = new List<Entity>().ToAsyncEnumerable().ToSCLArray();
        var stream2 = new List<Entity>().ToAsyncEnumerable().ToSCLArray();

        var dict = new Dictionary<RubyFunctionParameter, object>
        {
            { new RubyFunctionParameter("stream1Arg", "Stream1", false), stream1 },
            { new RubyFunctionParameter("stream2Arg", "Stream2", false), stream2 }
        };

        var stepParams = new ReadOnlyDictionary<RubyFunctionParameter, object>(dict);

        var step = new FakeNuixTwoStreamFunction();

        var result = await nuixConnection.RunFunctionAsync(
            ConnectionTestsHelper.GetStateMonadForProcess(logFactory),
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
        var nuixConnection = ConnectionTestsHelper.GetNuixConnection(logFactory, action);
        var ct             = new CancellationToken();

        var entities = new List<Entity>
        {
            Entity.Create(("Property1", "Value1")), Entity.Create(("Property2", "Value2"))
        };

        var stream1 = entities.ToAsyncEnumerable().ToSCLArray();

        var dict = new Dictionary<RubyFunctionParameter, object>
        {
            { new RubyFunctionParameter("entityStream", "EntityStream", false), stream1 }
        };

        var stepParams = new ReadOnlyDictionary<RubyFunctionParameter, object>(dict);

        var step = new FakeNuixStreamFunction();

        var result = await nuixConnection.RunFunctionAsync(
            ConnectionTestsHelper.GetStateMonadForProcess(logFactory),
            null,
            step.RubyScriptStepFactory.RubyFunction,
            stepParams,
            CasePathParameter.IgnoresOpenCase.Instance,
            ct
        );

        result.ShouldBeSuccessful();

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

        var action = ConnectionTestsHelper.SearchAndTagAction(casePath, search, tag, output);
        action.WriteToStdOut = stdOut;
        action.WriteToStdErr = stdErr;

        var loggerFactory = TestLoggerFactory.Create();

        var nuixConnection = ConnectionTestsHelper.GetNuixConnection(
            loggerFactory,
            ConnectionTestsHelper.SearchHelperAction,
            ConnectionTestsHelper.ExpandHelperAction,
            action
        );

        var result = await nuixConnection.RunFunctionAsync(
            ConnectionTestsHelper.GetStateMonadForProcess(loggerFactory),
            null,
            new NuixSearchAndTag().RubyScriptStepFactory.RubyFunction,
            ConnectionTestsHelper.SearchAndTagParams(casePath, search, tag),
            CasePathParameter.IgnoresOpenCase.Instance,
            new CancellationToken()
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

    [Fact]
    public async Task RunFunctionAsync_WhenResponseIsEntity_ReturnsEntity()
    {
        var outputData = Entity.Create(("Name", "workstation"), ("Description", "license"));

        var action = new ExternalProcessAction(
            new ConnectionCommand
            {
                Command = "GetLicenseDetails", FunctionDefinition = "", Arguments = new()
            },
            new ConnectionOutput { Result = new ConnectionOutputResult { Data = outputData } }
        );

        var lf = TestLoggerFactory.Create();

        var nuixConnection = ConnectionTestsHelper.GetNuixConnection(lf, action);

        var rubyParams = new ReadOnlyDictionary<RubyFunctionParameter, object>(
            new Dictionary<RubyFunctionParameter, object>()
        );

        var result = await nuixConnection.RunFunctionAsync(
            ConnectionTestsHelper.GetStateMonadForProcess(lf),
            null,
            new NuixGetLicenseDetails().RubyScriptStepFactory.RubyFunction,
            rubyParams,
            CasePathParameter.IgnoresOpenCase.Instance,
            new CancellationToken()
        );

        Assert.True(result.IsSuccess);
        Assert.IsType<Entity>(result.Value);

        Assert.Equal(
            "workstation",
            result.Value.TryGetValue("Name").GetValueOrThrow().GetPrimitiveString()
        );
    }
}
