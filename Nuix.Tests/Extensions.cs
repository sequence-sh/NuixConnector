using System.IO.Abstractions;
using Moq;
using Sequence.Core.TestHarness;

namespace Sequence.Connectors.Nuix.Tests;

public static class Extensions
{
    public static T WithScriptExists<T>(this T cws, bool success = true)
        where T : ICaseWithSetup
    {
        cws.ExternalContextSetupHelper.AddContextMock(
            ConnectorInjection.FileSystemKey,
            mr =>
            {
                var mock = mr.Create<IFileSystem>();

                mock.Setup(x => x.File.Exists(It.IsAny<string>())).Returns(success);

                return mock;
            }
        );

        return cws;
    }
}
