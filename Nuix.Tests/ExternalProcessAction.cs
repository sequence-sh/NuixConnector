using FluentAssertions;
using Reductech.Sequence.Connectors.Nuix.Steps.Meta.ConnectionObjects;

namespace Reductech.Sequence.Connectors.Nuix.Tests;

public class ExternalProcessAction
{
    public ExternalProcessAction(ConnectionCommand command, params ConnectionOutput[] desiredOutput)
    {
        Command       = command;
        DesiredOutput = desiredOutput;

        if (command.IsStream is null or false)
            DesiredOutput.Should().NotBeEmpty("If output is empty then the test will hang forever");
    }

    public ConnectionCommand Command { get; }

    public IReadOnlyList<ConnectionOutput> DesiredOutput { get; }

    public string[]? WriteToStdOut { get; set; }

    public string[]? WriteToStdErr { get; set; }
}
