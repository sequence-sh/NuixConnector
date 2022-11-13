using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using FluentAssertions;
using MELT;
using Sequence.Connectors.Nuix.Logging;
using Sequence.Core.Steps;
using Xunit;

namespace Sequence.Connectors.Nuix.Tests.Logging;

public class LogSituationNuixTests
{
    [Fact]
    public void LogSituationNuix_LogsCorrectMessage()
    {
        var lf = TestLoggerFactory.Create();

        var stateMonad = new StateMonad(
            lf.CreateLogger("test"),
            StepFactoryStore.Create(),
            null!,
            null!
        );

        var fields = typeof(LogSituationNuix).GetFields(BindingFlags.Static | BindingFlags.Public);

        foreach (var field in fields)
        {
            var situation = field.GetValue(null) as LogSituationNuix;

            if (situation == null)
                continue;

            var msg = LogMessages_EN.ResourceManager.GetString(situation.Code);
            Assert.NotNull(msg);

            // ReSharper disable once RemoveToList.1
            var msgParams = Regex.Matches(msg!, "\\{(.+?)\\}")
                .Select(m => m.Groups[1].Value)
                .ToList()
                .ToArray();

            var counter = 0;

            foreach (var p in msgParams)
                msg = msg!.Replace(p, counter++.ToString());

            var expected = string.Format(msg!, msgParams);

            situation.Log(stateMonad, new And(), msgParams);

            lf.Sink.LogEntries.Should()
                .Contain(
                    x => x.LogLevel == situation.LogLevel
                      && x.Message != null && x.Message.Equals(expected)
                );
        }
    }
}
