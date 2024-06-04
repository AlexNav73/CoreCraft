using CoreCraft.Persistence;
using CoreCraft.Persistence.History;

namespace CoreCraft.Storage.Sqlite;

/// <summary>
///     A SQLite storage implementation for the domain model
/// </summary>
public interface ISqliteStorage : IStorage, IHistoryStorage
{
}
