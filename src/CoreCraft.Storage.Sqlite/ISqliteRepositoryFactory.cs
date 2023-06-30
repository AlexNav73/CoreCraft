namespace CoreCraft.Storage.Sqlite;

internal interface ISqliteRepositoryFactory
{
    ISqliteRepository Create(string path, Action<string>? loggingAction = null);
}
