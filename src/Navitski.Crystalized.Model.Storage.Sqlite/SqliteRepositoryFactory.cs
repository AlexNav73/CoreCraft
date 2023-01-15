using System.Diagnostics.CodeAnalysis;

namespace Navitski.Crystalized.Model.Storage.Sqlite;

[ExcludeFromCodeCoverage]
internal sealed class SqliteRepositoryFactory : ISqliteRepositoryFactory
{
    public ISqliteRepository Create(string path, Action<string>? loggingAction = null)
    {
        return new SqliteRepository(path, loggingAction);
    }
}
