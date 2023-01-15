using Navitski.Crystalized.Model.Engine.Persistence;
using System.Data;

namespace Navitski.Crystalized.Model.Storage.Sqlite;

/// <summary>
///     SQLite extension of the base <see cref="IRepository"/> interface
/// </summary>
public interface ISqliteRepository : IRepository, IDisposable
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
    ///     Gets latests database version
    /// </summary>
    /// <returns>Version</returns>
    long GetDatabaseVersion();

    /// <summary>
    ///     Sets new database version
    /// </summary>
    /// <param name="version">New version</param>
    void SetDatabaseVersion(long version);
}
