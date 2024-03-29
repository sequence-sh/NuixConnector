﻿using System.IO.Abstractions;
using CSharpFunctionalExtensions;
using Sequence.Core.Connectors;
using Sequence.Core.Internal.Errors;

namespace Sequence.Connectors.Nuix;

/// <summary>
/// Provides context information for the nuix connector
/// </summary>
public sealed class ConnectorInjection : IConnectorInjection
{
    /// <summary>
    /// The key that this connector injection will use
    /// </summary>
    public const string FileSystemKey = "Nuix.FileSystem";

    /// <inheritdoc />
    public Result<IReadOnlyCollection<(string Name, object Context)>, IErrorBuilder>
        TryGetInjectedContexts()
    {
        IFileSystem fileSystem = new FileSystem();

        IReadOnlyCollection<(string Name, object Context)> list =
            new List<(string Name, object Context)> { (FileSystemKey, fileSystem) };

        return Result.Success<IReadOnlyCollection<(string Name, object Context)>, IErrorBuilder>(
            list
        );
    }
}
