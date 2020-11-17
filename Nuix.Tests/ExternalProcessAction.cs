using System.Collections.Generic;
using FluentAssertions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public class ExternalProcessAction
    {
        public ExternalProcessAction(ConnectionCommand command, params ConnectionOutput [] desiredOutput)
        {
            Command = command;
            DesiredOutput = desiredOutput;

            DesiredOutput.Should().NotBeEmpty("If output is empty then the test will hang forever");
        }

        public ConnectionCommand Command { get; }

        public IReadOnlyList<ConnectionOutput> DesiredOutput { get; }

    }
}