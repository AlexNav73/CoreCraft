using System.Diagnostics.CodeAnalysis;

namespace Navitski.Crystalized.Model.Storage.Sqlite;

/// <inheritdoc cref="ISqliteRepositoryFactory"/>
[ExcludeFromCodeCoverage]
public class SqliteRepositoryFactory : ISqliteRepositoryFactory
{
    /// <inheritdoc cref="ISqliteRepositoryFactory.Create(string)"/>
    public ISqliteRepository Create(string path)
    {
        return new SqliteRepository(path);
    }
}
