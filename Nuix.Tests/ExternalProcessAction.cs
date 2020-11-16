using System.Collections.Generic;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public class ExternalProcessAction
    {
        public ExternalProcessAction(ConnectionCommand command, params ConnectionOutput [] desiredOutput)
        {
            Command = command;
            DesiredOutput = desiredOutput;
        }

        public ConnectionCommand Command { get; }

        public IReadOnlyList<ConnectionOutput> DesiredOutput { get; }

    }
}