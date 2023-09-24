using CoreCraft.Persistence;
using CoreCraft.Storage.Sqlite.Migrations;
using System.Data;

namespace CoreCraft.Storage.Sqlite;

/// <summary>
///     A SQLite storage implementation for the domain model
/// </summary>
public sealed class SqliteStorage : IStorage
{
    private readonly MigrationRunner _migrationRunner;
    private readonly ISqliteRepositoryFactory _sqliteRepositoryFactory;
    private readonly Action<string>? _logginAction;

    /// <summary>
    ///     Ctor
    /// </summary>
    public SqliteStorage(
        IEnumerable<IMigration> migrations,
        Action<string>? logginAction = null)
        : this(migrations, new SqliteRepositoryFactory(), logginAction)
    {
    }

    internal SqliteStorage(
        IEnumerable<IMigration> migrations,
        ISqliteRepositoryFactory sqliteRepositoryFactory,
        Action<string>? logginAction = null)
    {
        _migrationRunner = new MigrationRunner(migrations);
        _sqliteRepositoryFactory = sqliteRepositoryFactory;
        _logginAction = logginAction;
    }

    /// <inheritdoc cref="IStorage.Update(string, IEnumerable{ICanBeSaved})"/>
    public void Update(string path, IEnumerable<ICanBeSaved> modelChanges)
    {
        Transaction(path, repository =>
        {
            foreach (var change in modelChanges)
            {
                change.Save(repository);
            }
        });
    }

    /// <inheritdoc cref="IStorage.Save(string, IEnumerable{ICanBeSaved})"/>
    public void Save(string path, IEnumerable<ICanBeSaved> modelShards)
    {
        Transaction(path, repository =>
        {
            _migrationRunner.UpdateDatabaseVersion(repository);

            foreach (var shard in modelShards)
            {
                shard.Save(repository);
            }
        });
    }

    /// <inheritdoc cref="IStorage.Load(string, IEnumerable{ICanBeLoaded})"/>
    public void Load(string path, IEnumerable<ICanBeLoaded> modelShards)
    {
        using var repository = _sqliteRepositoryFactory.Create(path, _logginAction);

        _migrationRunner.Run(repository);

        foreach (var loadable in modelShards)
        {
            loadable.Load(repository);
        }
    }

    private void Transaction(string path, Action<ISqliteRepository> action)
    {
        ISqliteRepository? repository = null;
        IDbTransaction? transaction = null;

        try
        {
            repository = _sqliteRepositoryFactory.Create(path, _logginAction);
            transaction = repository.BeginTransaction();

            action(repository);

            transaction.Commit();
        }
        catch (Exception ex)
        {
            transaction?.Rollback();
            _logginAction?.Invoke($"Exception was thrown with message: {ex.Message}");
            _logginAction?.Invoke($"Stack trace: {ex.StackTrace}");
            throw;
        }
        finally
        {
            transaction?.Dispose();
            repository?.Dispose();
        }
    }
}
