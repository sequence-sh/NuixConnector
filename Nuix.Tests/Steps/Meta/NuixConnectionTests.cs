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
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Abstractions;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.TestHarness;
using Reductech.EDR.Core.Util;
using Xunit;
using Entity = Reductech.EDR.Core.Entity;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps.Meta
{

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

        var searchHelperAction = new ExternalProcessAction(
            new ConnectionCommand { Command = "Search", FunctionDefinition = "", IsHelper = true },
            new ConnectionOutput { Result = new ConnectionOutputResult { Data = "helper_success" } }
        ) { WriteToStdOut = stdOut, WriteToStdErr = stdErr };

        var expandHelperAction = new ExternalProcessAction(
            new ConnectionCommand
            {
                Command = "ExpandSearch", FunctionDefinition = "", IsHelper = true
            },
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
            NuixConnectionTestsHelper.GetNuixConnection(
                loggerFactory,
                searchHelperAction,
                expandHelperAction,
                action
            );

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
