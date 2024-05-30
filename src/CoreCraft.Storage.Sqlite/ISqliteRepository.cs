using CoreCraft.Persistence;
using CoreCraft.Persistence.History;
using System.Data;

namespace CoreCraft.Storage.Sqlite;

/// <summary>
///     SQLite extension of the base <see cref="IRepository"/> interface
/// </summary>
public interface ISqliteRepository : IRepository, IHistoryRepository, IDisposable
{
    /// <summary>
    ///     Creates new transaction
    /// </summary>
    /// <returns>Created transaction</returns>
    IDbTransaction BeginTransaction();

    /// <summary>
    ///     Executes a query
    /// </summary>
    /// <param name="query">A query to execute</param>
    void ExecuteNonQuery(string query);

    /// <summary>
    ///     Gets latest database version
    /// </summary>
    /// <returns>Version</returns>
    int GetDatabaseVersion();

    /// <summary>
    ///     Sets new database version
    /// </summary>
    /// <param name="version">New version</param>
    void SetDatabaseVersion(int version);
}
