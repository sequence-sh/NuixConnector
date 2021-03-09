using System.Linq;
using FluentAssertions;
using MELT;
using Reductech.EDR.Connectors.Nuix.Logging;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Steps;
using Xunit;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;

namespace Reductech.EDR.Connectors.Nuix.Tests.Logging
{

public class LogSituationNuixTests
{
    [Fact]
    public void HelperExists_LogsMessage()
    {
        string name     = "FunctionName";
        string expected = $"Helper function {name} already exists.";

        var lf = TestLoggerFactory.Create();

        var stateMonad = new StateMonad(
            lf.CreateLogger("test"),
            NuixSettingsList.First(),
            null!,
            null!,
            null!
        );

        LogSituationNuix.HelperExists.Log(stateMonad, new And(), name);

        lf.Sink.LogEntries.Should()
            .Contain(
                x => x.LogLevel == LogSituationNuix.HelperExists.LogLevel
                  && x.Message != null && x.Message.Equals(expected)
            );
    }
}

}
