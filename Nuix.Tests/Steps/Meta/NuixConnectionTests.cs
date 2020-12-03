using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core;
using Reductech.EDR.Core.ExternalProcesses;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.TestHarness;
using Xunit;
using Xunit.Sdk;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps.Meta
{
    public class NuixConnectionHelperTests
    {
        private static StateMonad GetStateMonad(IExternalProcessRunner externalProcessRunner)
        {
            var nuixSettings = new NuixSettings(
                true,
                Constants.NuixSettingsList.First().NuixExeConsolePath,
                new Version(8, 8),
                Constants.AllNuixFeatures
            );
            
            var sfs = StepFactoryStore.CreateUsingReflection(typeof(IStep), typeof(IRubyScriptStep));

            var monad = new StateMonad(
                new TestLogger(), 
                nuixSettings,
                externalProcessRunner,
                FileSystemHelper.Instance,
                sfs
            );

            return monad;
        }

        private static IStateMonad GetStateMonadWithConnection()
        {
            var fakeExternalProcess = new ExternalProcessMock(2, GetCreateCaseAction());
            
            IStateMonad state = GetStateMonad(fakeExternalProcess);
            
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

        private static ExternalProcessAction GetCreateCaseAction()
        {
            return new ExternalProcessAction(new ConnectionCommand
                {
                    Command = "CreateCase",
                    FunctionDefinition = "",
                    Arguments = new Dictionary<string, object>
                    {
                        {nameof(NuixCreateCase.CasePath), "d:\\case"},
                        {nameof(NuixCreateCase.CaseName), "Integration Test Case"},
                        {nameof(NuixCreateCase.Investigator), "Mark"}
                    }
                },
                new ConnectionOutput
                {
                    Result = new ConnectionOutputResult {Data = null}
                }
            );
        }

        [Fact]
        public void GetOrCreateNuixConnection_WhenConnectionExists_ReturnsConnection()
        {
            var state = GetStateMonadWithConnection();
            
            var expected = state.GetVariable<NuixConnection>(NuixConnectionHelper.NuixVariableName);

            var createConnection = state.GetOrCreateNuixConnection(false);

            Assert.True(createConnection.IsSuccess);
            Assert.Same(expected.Value, createConnection.Value);
        }

        [Fact]
        public void GetOrCreateNuixConnection_WhenReopenIsSet_DisposesOldConnection()
        {
            var state = GetStateMonadWithConnection();
            
            var originalConnection = state.GetVariable<NuixConnection>(NuixConnectionHelper.NuixVariableName);

            var createConnection = state.GetOrCreateNuixConnection(true);

            var processRef = originalConnection.Value.ExternalProcess as ExternalProcessMock.ProcessReferenceMock;

            Assert.True(createConnection.IsSuccess);
            Assert.True(processRef!.IsDisposed);
        }
        
        [Fact]
        public void GetOrCreateNuixConnection_WhenConnectionAlreadyDisposed_LogsMessage()
        {
            var state = GetStateMonadWithConnection();

            var originalConnection = state.GetVariable<NuixConnection>(NuixConnectionHelper.NuixVariableName);
            
            originalConnection.Value.Dispose();

            var createConnection = state.GetOrCreateNuixConnection(true);

            Assert.True(originalConnection.IsSuccess);
            Assert.True(createConnection.IsSuccess);
            
            var logger = state.Logger as TestLogger;
            logger!.LoggedValues.Should().Contain(s =>
                s.ToString()!.Equals("Connection already disposed."));
        }

        [Fact]
        public void GetOrCreateNuixConnection_OnStartExternalProcessFailure_ReturnsError()
        {
            var fakeExternalProcess = new ExternalProcessMock(1, GetCreateCaseAction())
            {
                ProcessPath = "WrongPath",
                ValidateArguments = false
            };
            
            IStateMonad state = GetStateMonad(fakeExternalProcess);
            
            var createConnection = state.GetOrCreateNuixConnection(true);

            Assert.True(createConnection.IsFailure);
            Assert.Equal($"Could not start '{Constants.NuixSettingsList.First().NuixExeConsolePath}'", createConnection.Error.AsString);
        }

    }
}