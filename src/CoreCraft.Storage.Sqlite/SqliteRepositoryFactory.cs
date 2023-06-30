using System.Diagnostics.CodeAnalysis;

namespace CoreCraft.Storage.Sqlite;

[ExcludeFromCodeCoverage]
internal sealed class SqliteRepositoryFactory : ISqliteRepositoryFactory
{
    public ISqliteRepository Create(string path, Action<string>? loggingAction = null)
    {
        return new SqliteRepository(path, loggingAction);
    }
}
