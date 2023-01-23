namespace Navitski.Crystalized.Model.Storage.Sqlite;

internal interface ISqliteRepositoryFactory
{
    ISqliteRepository Create(string path, Action<string>? loggingAction = null);
}
