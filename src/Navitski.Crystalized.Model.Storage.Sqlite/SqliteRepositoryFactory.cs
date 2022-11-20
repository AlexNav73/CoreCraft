using System.Diagnostics.CodeAnalysis;

namespace Navitski.Crystalized.Model.Storage.Sqlite;

/// <inheritdoc cref="ISqliteRepositoryFactory"/>
[ExcludeFromCodeCoverage]
public sealed class SqliteRepositoryFactory : ISqliteRepositoryFactory
{
    /// <inheritdoc cref="ISqliteRepositoryFactory.Create(string, Action{string}?)"/>
    public ISqliteRepository Create(string path, Action<string>? loggingAction = null)
    {
        return new SqliteRepository(path, loggingAction);
    }
}
